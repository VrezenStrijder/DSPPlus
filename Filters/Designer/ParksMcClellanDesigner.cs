using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    /// <summary>
    /// Parks-McClellan 等波纹 FIR 滤波器设计器
    /// 支持任意带通、带阻、低通、高通滤波器的最优滤波器设计(L∞ 范数最小化)
    /// 
    /// 使用示例：
    /// var designer = new ParksMcClellanDesigner(order, freqs, desired, weights);
    /// var kernel = designer.Design();
    /// </summary>
    public class ParksMcClellanDesigner
    {
        //自定义计算时允许的最小误差(避免除零)
        private const double Tolerance = 1e-7;

        /// 当前使用的极值点索引(Extremal Frequencies); 表示误差函数的局部最大/最小点的位置
        private readonly int[] extrs;

        // 频率采样网格(通常按各频段均匀采样)
        private double[] grid;

        // 所有频带的边缘(按 [f1, f2, f3, f4, ...] 格式，归一化到 [0, 0.5]);必须是偶数个，表示多个频段
        private readonly double[] freqs;

        // 在整个网格上的期望幅度响应值(interpolation target)
        private double[] desired;

        // 在整个网格上的误差权重
        private double[] weights;

        // 当前迭代中的插值响应点(根据等波纹理论)
        private readonly double[] points;

        // Lagrange 插值法所用的 gamma 系数
        private readonly double[] gammas;

        // 极值频率对应的余弦值(简化公式计算)
        private readonly double[] cosTable;

        /// <summary>
        /// 构造函数，初始化频率点、期望响应和权重
        /// </summary>
        /// <param name="order">滤波器阶数(必须为奇数)</param>
        /// <param name="freqs">频带边界(归一化)，必须是偶数个，0~0.5</param>
        /// <param name="desired">每个频带的期望响应(例如 LowPass 是 [1, 0])</param>
        /// <param name="weights">每个频带的权重</param>
        /// <param name="gridDensity">每带频率网格密度(默认16)</param>
        public ParksMcClellanDesigner(int order, double[] freqs, double[] desired, double[] weights, int gridDensity = 16)
        {
            if (order % 2 == 0)
            {
                throw new ArgumentException($"order 必须是奇数");
            }

            int n = freqs.Length;

            if (n < 4 || n % 2 != 0)
            {
                throw new ArgumentException("频率数值的个数必须是偶数，且大于等于4");
            }
            if (freqs[0] != 0 || freqs[n - 1] != 0.5)
            {
                throw new ArgumentException("频率数组必须从 0 开始，到 0.5 结束");
            }
            if (desired.Length != (n / 2) || weights.Length != (n / 2))
            {
                throw new ArgumentException($"desired 和 weights 数组长度必须等于频段数量: {n / 2}");
            }

            Order = order;
            K = (Order / 2) + 2;

            this.freqs = freqs;

            MakeGrid(desired, weights, gridDensity);

            InterpolatedResponse = new double[grid.Length];
            Error = new double[grid.Length];

            extrs = new int[K];
            points = new double[K];
            gammas = new double[K];
            cosTable = new double[K];
        }

        /// <summary>
        /// 滤波器阶数
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// 当前设计执行的迭代次数
        /// </summary>
        public int Iterations { get; private set; }

        /// <summary>
        /// 极值点数量(K = Order / 2 + 2)固定公式
        /// </summary>
        public int K { get; private set; }

        /// <summary>
        /// 插值得到的实际频率响应
        /// </summary>
        public double[] InterpolatedResponse { get; private set; }

        /// <summary>
        /// 频率网格上的误差估计值
        /// </summary>
        public double[] Error { get; private set; }

        /// <summary>
        /// 当前极值频率点(用于画出等波纹结构)
        /// </summary>
        public double[] ExtremalFrequencies => extrs.Select(e => grid[e]).ToArray();

        /// <summary>
        /// 构建频率网格，并分配 desired 和 weight 数组到整个网格
        /// </summary>
        private void MakeGrid(double[] desired, double[] weights, int gridDensity = 16)
        {
            int gridSize = 0;
            int[] bandSizes = new int[freqs.Length / 2];
            double step = 0.5 / (gridDensity * (K - 1));  // 每带的采样间隔

            // 第一步：计算每个频带的采样点数量
            for (int i = 0; i < bandSizes.Length; i++)
            {
                bandSizes[i] = (int)(((freqs[(2 * i) + 1] - freqs[2 * i]) / step) + 0.5);
                gridSize += bandSizes[i];
            }

            grid = new double[gridSize];
            this.weights = new double[gridSize];
            this.desired = new double[gridSize];

            int gi = 0;

            // 第二步：分布desired和weight到整个grid
            for (int i = 0; i < bandSizes.Length; i++)
            {
                double freq = freqs[2 * i];

                for (int k = 0; k < bandSizes[i]; k++, gi++, freq += step)
                {
                    grid[gi] = freq;
                    this.weights[gi] = weights[i];
                    this.desired[gi] = desired[i];
                }

                grid[gi - 1] = freqs[(2 * i) + 1]; // 保证尾部精确对齐
            }
        }

        /// <summary>
        /// 初始化极值点索引，均匀取 K 个采样点作为初始极值点
        /// </summary>
        private void InitExtrema()
        {
            int n = grid.Length;
            for (int k = 0; k < K; k++)
            {
                extrs[k] = (int)(k * (n - 1.0) / (K - 1));
            }
        }

        /// <summary>
        /// 迭代执行 Remez 算法设计过程，并返回 FIR 滤波器系数
        /// </summary>
        /// <param name="maxIterations">最大迭代次数</param>
        public double[] Design(int maxIterations = 100)
        {
            InitExtrema();
            int[] extrCandidates = new int[2 * K];

            for (Iterations = 0; Iterations < maxIterations; Iterations++)
            {
                UpdateCoefficients();

                for (int i = 0; i < grid.Length; i++)
                {
                    InterpolatedResponse[i] = Lagrange(grid[i]);
                }
                for (int i = 0; i < grid.Length; i++)
                {
                    Error[i] = weights[i] * (desired[i] - InterpolatedResponse[i]);
                }
                int extrCount = 0;
                int n = grid.Length;

                // 查找误差函数的局部极大值点
                if (Math.Abs(Error[0]) > Math.Abs(Error[1]))
                {
                    extrCandidates[extrCount++] = 0;
                }
                for (int i = 1; i < n - 1; i++)
                {
                    if ((Error[i] > 0.0 && Error[i] >= Error[i - 1] && Error[i] > Error[i + 1]) ||
                        (Error[i] < 0.0 && Error[i] <= Error[i - 1] && Error[i] < Error[i + 1]))
                    {
                        extrCandidates[extrCount++] = i;
                    }
                }

                if (Math.Abs(Error[n - 1]) > Math.Abs(Error[n - 2]))
                {
                    extrCandidates[extrCount++] = n - 1;
                }
                if (extrCount < K)
                {
                    break;
                }
                // 超过 K 个极点时，删除误差最小的点
                while (extrCount > K)
                {
                    int indexToRemove = 0;
                    for (int i = 1; i < extrCount; i++)
                    {
                        if (Math.Abs(Error[extrCandidates[i]]) < Math.Abs(Error[extrCandidates[indexToRemove]]))
                        {
                            indexToRemove = i;
                        }
                    }

                    extrCount--;
                    for (int i = indexToRemove; i < extrCount; i++)
                    {
                        extrCandidates[i] = extrCandidates[i + 1];
                    }
                }

                Array.Copy(extrCandidates, extrs, K);

                double maxError = Error[extrs[0]];
                double minError = maxError;

                for (int k = 0; k < K; k++)
                {
                    double error = Math.Abs(Error[extrs[k]]);
                    if (error < minError) minError = error;
                    if (error > maxError) maxError = error;
                }

                if ((maxError - minError) / minError < 1e-6)
                {
                    break;
                }
            }

            return ImpulseResponse();
        }

        /// <summary>
        /// 更新 gamma 系数和插值点(用于下一轮插值和误差计算)
        /// </summary>
        private void UpdateCoefficients()
        {
            for (int i = 0; i < cosTable.Length; i++)
            {
                cosTable[i] = Math.Cos(2 * Math.PI * grid[extrs[i]]);
            }

            double num = 0.0, den = 0.0;
            for (int i = 0, sign = 1; i < K; i++, sign = -sign)
            {
                gammas[i] = Gamma(i);
                num += gammas[i] * desired[extrs[i]];
                den += sign * gammas[i] / weights[extrs[i]];
            }

            double delta = num / den;

            for (int i = 0, sign = 1; i < K; i++, sign = -sign)
            {
                points[i] = desired[extrs[i]] - (sign * delta / weights[extrs[i]]);
            }
        }

        /// <summary>
        /// 获取滤波器的冲激响应(即系数序列)
        /// </summary>
        private double[] ImpulseResponse()
        {
            UpdateCoefficients();
            int halfOrder = Order / 2;

            double[] lagr = Enumerable.Range(0, halfOrder + 1)
                               .Select(i => Lagrange((double)i / Order))
                               .ToArray();

            double[] kernel = new double[Order];

            for (int k = 0; k < Order; k++)
            {
                double sum = 0.0;
                for (int i = 1; i <= halfOrder; i++)
                {
                    sum += lagr[i] * Math.Cos(2 * Math.PI * i * (k - halfOrder) / Order);
                }
                kernel[k] = (lagr[0] + 2 * sum) / Order;
            }

            return kernel;
        }

        /// <summary>
        /// 计算 Gamma 系数，用于 Lagrange 插值权重
        /// </summary>
        private double Gamma(int k)
        {
            int jet = ((K - 1) / 15) + 1;
            double den = 1.0;

            for (int j = 0; j < jet; j++)
            {
                for (int i = j; i < K; i += jet)
                {
                    if (i != k)
                    {
                        den *= 2 * (cosTable[k] - cosTable[i]);
                    }
                }
            }

            return Math.Abs(den) < Tolerance ? 1.0 / Tolerance : 1.0 / den;
        }

        /// <summary>
        /// Baricentric Lagrange 插值器，用于计算频率响应
        /// </summary>
        private double Lagrange(double freq)
        {
            double num = 0.0, den = 0.0;
            double cosFreq = Math.Cos(2 * Math.PI * freq);

            for (int i = 0; i < K; i++)
            {
                double cosDiff = cosFreq - cosTable[i];

                if (Math.Abs(cosDiff) < Tolerance)
                {
                    return points[i];
                }
                double gammaOverDiff = gammas[i] / cosDiff;
                den += gammaOverDiff;
                num += gammaOverDiff * points[i];
            }

            return num / den;
        }

        /// <summary>
        /// 将通带纹波 dB 值转换为权重
        /// dB越小，权重应越大
        /// </summary>
        public static double DbToPassbandWeight(double db)
        {
            return (Math.Pow(10, db / 20) - 1) / (Math.Pow(10, db / 20) + 1);
        }

        /// <summary>
        /// 将阻带衰减 dB 值转换为权重
        /// </summary>
        public static double DbToStopbandWeight(double db)
        {
            return Math.Pow(10, -db / 20);
        }

        /// <summary>
        /// 使用文献公式估算低通滤波器阶数
        /// </summary>
        public static int EstimateOrder(double fp, double fa, double dp, double da)
        {
            if (dp < da)
            {
                double tmp = dp;
                dp = da;
                da = tmp;
            }

            double bw = fa - fp;

            double d = (((0.005309 * Math.Log10(dp) * Math.Log10(dp)) + (0.07114 * Math.Log10(dp)) - 0.4761) * Math.Log10(da))
                     - ((0.00266 * Math.Log10(dp) * Math.Log10(dp)) + (0.5941 * Math.Log10(dp)) + 0.4278);

            double f = (0.51244 * (Math.Log10(dp) - Math.Log10(da))) + 11.012;

            int l = (int)(((d - (f * bw * bw)) / bw) + 1.5);

            return l % 2 == 1 ? l : l + 1;
        }

        /// <summary>
        /// 多带通场景下估算阶数
        /// </summary>
        public static int EstimateOrder(double[] freqs, double[] deltas)
        {
            int maxOrder = 0;
            for (int fi = 1, di = 0; di < deltas.Length - 1; fi += 2, di++)
            {
                int order = EstimateOrder(freqs[fi], freqs[fi + 1], deltas[di], deltas[di + 1]);
                if (order > maxOrder)
                {
                    maxOrder = order;
                }
            }

            return maxOrder;
        }
    }
}
