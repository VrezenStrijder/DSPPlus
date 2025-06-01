using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MathNet.Filtering.IIR;
using MathNet.Numerics;

namespace DSPPlus.Filters
{
    public static class BesselFilterDesigner
    {
        public static IFilter DesignFilter(FilterParameter param)
        {
            var freqBand = param.FrequencyBands.FirstOrDefault();
            var type = freqBand.FrequencyType;

            switch (type)
            {
                case FrequencyFilterType.LowPass:
                    return DesignLowPass(param.Order, freqBand.Cutoff1, param.SampleRate);

                case FrequencyFilterType.HighPass:
                    return DesignHighPass(param.Order, freqBand.Cutoff1, param.SampleRate);

                case FrequencyFilterType.BandPass:
                    return DesignBandPass(param.Order, param.CenterFreq.Value, param.Bandwidth.Value, param.SampleRate);

                case FrequencyFilterType.BandStop:
                    return DesignBandStop(param.Order, param.CenterFreq.Value, param.Bandwidth.Value, param.SampleRate);

                default:
                    return null;
            }
        }

        public static IIRFilter DesignLowPass(int order, double cutoff, double sampleRate)
        {
            double warped = 2 * sampleRate * Math.Tan(Math.PI * cutoff / sampleRate);

            //获取模拟域极点
            Complex[] analogPoles = GetPoles(order).Select(p => p * warped).ToArray(); //支持更高阶可替换为GetPoles()方法

            //将模拟极点转换为数字极点(双线性变换)
            Complex[] digitalPoles = analogPoles
                .Select(p => (1.0 + p / (2.0 * sampleRate)) / (1.0 - p / (2.0 * sampleRate)))
                .ToArray();

            //计算传递函数系数
            var (b, a) = ZpkUtilities.ZpkToTf(new Complex[0], digitalPoles, 1.0);

            return new IIRFilter(new OnlineIirFilter(b.Concat(a).ToArray()));
        }

        public static IIRFilter DesignHighPass(int order, double cutoff, double sampleRate)
        {
            double warped = 2 * sampleRate * Math.Tan(Math.PI * cutoff / sampleRate);
            Complex[] analogPoles = GetPoles(order).Select(p => warped / p).ToArray();

            Complex[] digitalPoles = analogPoles
                .Select(p => (1.0 + p / (2.0 * sampleRate)) / (1.0 - p / (2.0 * sampleRate)))
                .ToArray();

            var (b, a) = ZpkUtilities.ZpkToTf(new Complex[0], digitalPoles, 1.0);
            return new IIRFilter(new OnlineIirFilter(b.Concat(a).ToArray()));
        }

        public static IIRFilter DesignBandPass(int order, double centerFreq, double bandwidth, double sampleRate)
        {
            double warpedCenter = 2 * sampleRate * Math.Tan(Math.PI * centerFreq / sampleRate);
            double warpedBandwidth = 2 * sampleRate * Math.Tan(Math.PI * bandwidth / sampleRate);

            var analogLowPassPoles = GetPoles(order);
            var analogPoles = analogLowPassPoles
                .SelectMany(p =>
                {
                    // s -> (s^2 + w0^2) / (B * s)
                    Complex s = p;
                    Complex temp = Complex.Sqrt(s * s - (warpedCenter * warpedCenter));
                    return new Complex[]
                    {
                        0.5 * warpedBandwidth * s + 0.5 * warpedBandwidth * temp,
                        0.5 * warpedBandwidth * s - 0.5 * warpedBandwidth * temp
                    };
                }).ToArray();

            var digitalPoles = analogPoles
                .Select(p => (1.0 + p / (2.0 * sampleRate)) / (1.0 - p / (2.0 * sampleRate)))
                .ToArray();

            var (b, a) = ZpkUtilities.ZpkToTf(new Complex[0], digitalPoles, 1.0);
            return new IIRFilter(new OnlineIirFilter(b.Concat(a).ToArray()));
        }

        public static IIRFilter DesignBandStop(int order, double centerFreq, double bandwidth, double sampleRate)
        {
            double warpedCenter = 2 * sampleRate * Math.Tan(Math.PI * centerFreq / sampleRate);
            double warpedBandwidth = 2 * sampleRate * Math.Tan(Math.PI * bandwidth / sampleRate);

            var analogLowPassPoles = GetPoles(order);
            var analogPoles = analogLowPassPoles
                .SelectMany(p =>
                {
                    // s -> (B * s) / (s^2 + w0^2)
                    Complex s = p;
                    Complex temp = Complex.Sqrt((warpedCenter * warpedCenter) - s * s);
                    return new Complex[]
                    {
                        0.5 * warpedBandwidth / s + 0.5 * warpedBandwidth / temp,
                        0.5 * warpedBandwidth / s - 0.5 * warpedBandwidth / temp
                    };
                }).ToArray();

            var digitalPoles = analogPoles
                .Select(p => (1.0 + p / (2.0 * sampleRate)) / (1.0 - p / (2.0 * sampleRate)))
                .ToArray();

            var (b, a) = ZpkUtilities.ZpkToTf(new Complex[0], digitalPoles, 1.0);
            return new IIRFilter(new OnlineIirFilter(b.Concat(a).ToArray()));
        }

        /// <summary>
        /// 预定义模拟 Bessel 滤波器极点
        /// </summary>
        private static Complex[] GetBesselAnalogPoles(int order)
        {
            switch (order)
            {
                case 2:
                    return new Complex[]
                    {
                    new Complex(-1.1016, 0),
                    new Complex(-0.5385, 0)
                    };
                case 3:
                    return new Complex[]
                    {
                    new Complex(-1.3227, 0),
                    new Complex(-0.5622, 0.7547),
                    new Complex(-0.5622, -0.7547)
                    };
                case 4:
                    return new Complex[]
                    {
                    new Complex(-1.3701, 0),
                    new Complex(-0.4103, 0.9040),
                    new Complex(-0.4103, -0.9040),
                    new Complex(-0.8220, 0)
                    };
                case 5:
                    return new Complex[]
                    {
                    new Complex(-1.5023, 0),
                    new Complex(-0.3880, 1.0629),
                    new Complex(-0.3880, -1.0629),
                    new Complex(-0.7560, 0.3700),
                    new Complex(-0.7560, -0.3700)
                    };
                default:
                    throw new NotImplementedException("Bessel filter only supports order 2–5 for now.");
            }
        }


        public static Complex[] GetPoles(int order)
        {
            if (order >= 2 && order <= 5)
            {
                return GetBesselAnalogPoles(order);
            }
            else if (order > 0 && order <= 10)
            {
                // 获取 Bessel 多项式系数（从高次到低次）
                var coeffs = GetCoefficients(order);

                var roots = FindRoots.Polynomial(coeffs);

                // FindRoots 返回 Complex[]，即使是实根也封装为复数
                return roots.OrderBy(p => p.Real).ToArray();
            }

            throw new ArgumentException($"尚未支持阶数为 {order} 的多项式系数");
        }

        /// <summary>
        /// 获取阶数为 n 的 Bessel 多项式系数（从高次到低次）
        /// </summary>
        private static double[] GetCoefficients(int n)
        {
            var table = new Dictionary<int, double[]>
            {
                {1, new double[] {1, 1}},
                {2, new double[] {1, 3, 3}},
                {3, new double[] {1, 6, 15, 15}},
                {4, new double[] {1, 10, 45, 105, 105}},
                {5, new double[] {1, 15, 105, 420, 945, 945}},
                {6, new double[] {1, 21, 210, 1260, 4725, 10395, 10395}},
                {7, new double[] {1, 28, 378, 3150, 17325, 62370, 135135, 135135}},
                {8, new double[] {1, 36, 630, 6930, 51975, 270270, 945945, 2027025, 2027025}},
                {9, new double[] {1, 45, 990, 13860, 135135, 945945, 4729725, 16216200, 34459425, 34459425}},
                {10, new double[] {1, 55, 1485, 25740, 310310, 2702700, 16216200, 68918850, 189189000, 344594250, 344594250}},
            };

            return table[n];
        }


    }
}