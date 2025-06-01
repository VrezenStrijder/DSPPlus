using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Filtering.IIR;

namespace DSPPlus.Filters
{
    /// <summary>
    /// IIR 滤波器
    /// (适用相位不敏感,精度相对低,计算资源受限的情况)
    /// </summary>
    public class IIRFilter : IFilter
    {
        private readonly double[] a; // 分母系数（反馈）
        private readonly double[] b; // 分子系数（前馈）
        private readonly double[] x; // 输入历史
        private readonly double[] y; // 输出历史

        private readonly OnlineIirFilter internalFilter;

        /// <summary>
        /// 使用 a[], b[] 系数构造
        /// </summary>
        public IIRFilter(double[] numerator, double[] denominator)
        {
            b = numerator;
            a = denominator;
            x = new double[b.Length];
            y = new double[a.Length];
        }

        /// <summary>
        /// 使用 MathNet 的 OnlineIirFilter 构造
        /// </summary>
        public IIRFilter(OnlineIirFilter filter)
        {
            internalFilter = filter ?? throw new ArgumentNullException(nameof(filter));
        }


        public double Process(double input)
        {
            //if (internalFilter != null)
            //{
            //    return internalFilter.ProcessSample(input);
            //}

            // 移动输入缓冲区
            for (int i = x.Length - 1; i > 0; i--)
            {
                x[i] = x[i - 1];
            }
            x[0] = input;

            // 移动输出缓冲区
            for (int i = y.Length - 1; i > 0; i--)
            {
                y[i] = y[i - 1];
            }
            // 计算新输出 y[n] = (b[0]*x[n] + b[1]*x[n-1] + ... + b[M]*x[n-M]) - (a[1]*y[n-1] + ... + a[N]*y[n-N])
            double output = 0;
            for (int i = 0; i < b.Length; i++)
            {
                output += b[i] * x[i];
            }
            for (int i = 1; i < a.Length; i++)
            {
                output -= a[i] * y[i - 1];
            }
            // 归一化
            output /= a[0];
            y[0] = output;

            return output;
        }

        public double[] ProcessBatch(double[] inputSignal)
        {
            //if (internalFilter != null)
            //{
            //    return internalFilter.ProcessSamples(inputSignal);
            //}

            double[] output = new double[inputSignal.Length];

            // 重置滤波器状态
            Array.Clear(x, 0, x.Length);
            Array.Clear(y, 0, y.Length);

            for (int i = 0; i < inputSignal.Length; i++)
            {
                output[i] = Process(inputSignal[i]);
            }

            return output;
        }

        /// <summary>
        /// 低通滤波器设计
        /// (巴特沃斯 Butterworth)
        /// </summary>
        /// <param name="cutoffFreq"></param>
        /// <param name="sampleRate"></param>
        /// <param name="order">阶数(分母多项式的最高次数,表示差分方程中反馈项(延迟输出样本)的最大延迟)</param>
        /// <returns></returns>
        public static IIRFilter DoLowPass(double cutoffFreq, double sampleRate, int order)
        {
            if (order % 2 != 0)
            {
                order++; // 确保阶数为偶数，方便实现
            }
            // 归一化截止频率 (0 到 1 之间)
            double omega = 2.0 * Math.PI * cutoffFreq / sampleRate;
            double alpha = Math.Sin(omega) / (2.0 * GetQFactor(order));

            // 二阶部分设计
            double[] a = new double[order + 1];
            double[] b = new double[order + 1];

            // 初始化
            a[0] = 1.0;
            b[0] = 1.0;

            // 简化的巴特沃斯设计 (二阶部分级联)
            for (int i = 0; i < order / 2; i++)
            {
                double theta = Math.PI * (2.0 * i + 1) / (2.0 * order);
                double real = -Math.Sin(theta);
                double imag = Math.Cos(theta);

                // 双二次变换系数
                double b0 = (1.0 - Math.Cos(omega)) / 2.0;
                double b1 = 1.0 - Math.Cos(omega);
                double b2 = (1.0 - Math.Cos(omega)) / 2.0;
                double a0 = 1.0 + alpha;
                double a1 = -2.0 * Math.Cos(omega);
                double a2 = 1.0 - alpha;

                // 归一化
                b0 /= a0; b1 /= a0; b2 /= a0;
                a1 /= a0; a2 /= a0;

                // 设置最终系数
                // (简化实现，完整实现需要多极点卷积)
                b[0] = b0;
                b[1] = b1;
                b[2] = b2;
                a[0] = 1.0;
                a[1] = a1;
                a[2] = a2;
            }

            return new IIRFilter(b, a);
        }

        /// <summary>
        /// 高通滤波器设计
        /// </summary>
        /// <param name="cutoffFreq"></param>
        /// <param name="sampleRate"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static IIRFilter DoHighPass(double cutoffFreq, double sampleRate, int order)
        {
            if (order % 2 != 0)
            {
                order++;
            }
            double omega = 2.0 * Math.PI * cutoffFreq / sampleRate;
            double alpha = Math.Sin(omega) / (2.0 * GetQFactor(order));

            double[] a = new double[order + 1];
            double[] b = new double[order + 1];

            a[0] = 1.0;

            // 高通变换的系数设计
            double b0 = (1.0 + Math.Cos(omega)) / 2.0;
            double b1 = -(1.0 + Math.Cos(omega));
            double b2 = (1.0 + Math.Cos(omega)) / 2.0;
            double a0 = 1.0 + alpha;
            double a1 = -2.0 * Math.Cos(omega);
            double a2 = 1.0 - alpha;

            // 归一化
            b0 /= a0; b1 /= a0; b2 /= a0;
            a1 /= a0; a2 /= a0;

            b[0] = b0;
            b[1] = b1;
            b[2] = b2;
            a[0] = 1.0;
            a[1] = a1;
            a[2] = a2;

            return new IIRFilter(b, a);
        }

        /// <summary>
        /// 带通滤波器设计
        /// </summary>
        /// <param name="lowCutoffFreq"></param>
        /// <param name="highCutoffFreq"></param>
        /// <param name="sampleRate"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static IIRFilter DoBandPass(double lowCutoffFreq, double highCutoffFreq, double sampleRate, int order)
        {
            if (order % 2 != 0)
            {
                order++;
            }
            double omega1 = 2.0 * Math.PI * lowCutoffFreq / sampleRate;
            double omega2 = 2.0 * Math.PI * highCutoffFreq / sampleRate;
            double centerFreq = (omega1 + omega2) / 2.0;
            double bandwidth = omega2 - omega1;
            double alpha = Math.Sin(bandwidth) / (2.0 * GetQFactor(order));

            double[] a = new double[order + 1];
            double[] b = new double[order + 1];

            a[0] = 1.0;

            // 带通变换设计
            double b0 = alpha;
            double b1 = 0;
            double b2 = -alpha;
            double a0 = 1.0 + alpha;
            double a1 = -2.0 * Math.Cos(centerFreq);
            double a2 = 1.0 - alpha;

            // 归一化
            b0 /= a0; b1 /= a0; b2 /= a0;
            a1 /= a0; a2 /= a0;

            b[0] = b0;
            b[1] = b1;
            b[2] = b2;
            a[0] = 1.0;
            a[1] = a1;
            a[2] = a2;

            return new IIRFilter(b, a);
        }

        /// <summary>
        /// 阻带滤波器设计
        /// </summary>
        /// <param name="lowCutoffFreq"></param>
        /// <param name="highCutoffFreq"></param>
        /// <param name="sampleRate"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static IIRFilter DoBandStop(double lowCutoffFreq, double highCutoffFreq, double sampleRate, int order)
        {
            if (order % 2 != 0)
            {
                order++;
            }
            double omega1 = 2.0 * Math.PI * lowCutoffFreq / sampleRate;
            double omega2 = 2.0 * Math.PI * highCutoffFreq / sampleRate;
            double centerFreq = (omega1 + omega2) / 2.0;
            double bandwidth = omega2 - omega1;
            double alpha = Math.Sin(bandwidth) / (2.0 * GetQFactor(order));

            double[] a = new double[order + 1];
            double[] b = new double[order + 1];

            a[0] = 1.0;

            // 阻带(带阻)变换设计
            double b0 = 1.0;
            double b1 = -2.0 * Math.Cos(centerFreq);
            double b2 = 1.0;
            double a0 = 1.0 + alpha;
            double a1 = -2.0 * Math.Cos(centerFreq);
            double a2 = 1.0 - alpha;

            // 归一化
            b0 /= a0; b1 /= a0; b2 /= a0;
            a1 /= a0; a2 /= a0;

            b[0] = b0;
            b[1] = b1;
            b[2] = b2;
            a[0] = 1.0;
            a[1] = a1;
            a[2] = a2;

            return new IIRFilter(b, a);
        }

        private static double GetQFactor(int order)
        {
            // 巴特沃斯滤波器设计中Q因子的近似计算
            return 1.0 / (2.0 * Math.Sin(Math.PI / (order * 2)));
        }
    }


}
