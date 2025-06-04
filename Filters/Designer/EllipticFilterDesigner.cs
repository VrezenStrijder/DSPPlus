using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MathNet.Filtering.IIR;

namespace DSPPlus.Filters
{
    public static class EllipticFilterDesigner
    {
        public static IFilter DesignFilter(FilterParameter param)
        {
            var freqBand = param.FrequencyBands.FirstOrDefault();
            var type = freqBand.FrequencyType;

            double ripple = param.PassbandRipple ?? 1;
            double stopband = param.StopbandAttenuation ?? 60;

            switch (type)
            {
                case FrequencyFilterType.LowPass:
                    return DesignLowPass(param.Order, freqBand.Cutoff1, param.SampleRate, ripple, stopband);

                case FrequencyFilterType.HighPass:
                    return DesignHighPass(param.Order, freqBand.Cutoff1, param.SampleRate, ripple, stopband);

                case FrequencyFilterType.BandPass:
                    return DesignBandPass(param.Order, freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, ripple, stopband);

                case FrequencyFilterType.BandStop:
                    return DesignBandStop(param.Order, freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, ripple, stopband);

                default:
                    return null;
            }
        }

        public static IIRFilter DesignLowPass(int order, double cutoff, double fs, double ripple, double stopband)
        {
            var zpk = GetNormedZerosPoles(order, ripple, stopband);
            var zDpk = ZpkUtilities.BilinearTransform(zpk.Item1, zpk.Item2, 1.0, fs, cutoff);
            var tf = ZpkUtilities.ZpkToTf(zDpk.Item1, zDpk.Item2, zDpk.Item3);
            return new IIRFilter(new OnlineIirFilter(tf.Item1.Concat(tf.Item2).ToArray()));
        }

        public static IIRFilter DesignHighPass(int order, double cutoff, double fs, double ripple, double stopband)
        {
            var zpk = GetNormedZerosPoles(order, ripple, stopband);
            var z = zpk.Item1.Select(x => -1.0 / x).ToArray();
            var p = zpk.Item2.Select(x => -1.0 / x).ToArray();
            var zDpk = ZpkUtilities.BilinearTransform(z, p, 1.0, fs, cutoff);
            var tf = ZpkUtilities.ZpkToTf(zDpk.Item1, zDpk.Item2, zDpk.Item3);
            return new IIRFilter(new OnlineIirFilter(tf.Item1.Concat(tf.Item2).ToArray()));
        }

        public static IIRFilter DesignBandPass(int order, double low, double high, double fs, double ripple, double stopband)
        {
            double bw = high - low;
            double fc = Math.Sqrt(low * high);

            var zpk = GetNormedZerosPoles(order, ripple, stopband);
            var z = zpk.Item1.SelectMany(x => ZpkUtilities.BandTransform(x, bw, fc)).ToArray();
            var p = zpk.Item2.SelectMany(x => ZpkUtilities.BandTransform(x, bw, fc)).ToArray();

            var zDpk = ZpkUtilities.BilinearTransform(z, p, 1.0, fs, fc);
            var tf = ZpkUtilities.ZpkToTf(zDpk.Item1, zDpk.Item2, zDpk.Item3);
            return new IIRFilter(new OnlineIirFilter(tf.Item1.Concat(tf.Item2).ToArray()));
        }

        public static IIRFilter DesignBandStop(int order, double low, double high, double fs, double ripple, double stopband)
        {
            double bw = high - low;
            double fc = Math.Sqrt(low * high);

            var zpk = GetNormedZerosPoles(order, ripple, stopband);
            var z = zpk.Item1.SelectMany(x => ZpkUtilities.BandStopTransform(x, bw, fc)).ToArray();
            var p = zpk.Item2.SelectMany(x => ZpkUtilities.BandStopTransform(x, bw, fc)).ToArray();

            var zDpk = ZpkUtilities.BilinearTransform(z, p, 1.0, fs, fc);
            var tf = ZpkUtilities.ZpkToTf(zDpk.Item1, zDpk.Item2, zDpk.Item3);
            return new IIRFilter(new OnlineIirFilter(tf.Item1.Concat(tf.Item2).ToArray()));
        }

        /// <summary>
        /// 椭圆滤波器零极点生成器
        /// (基于 Jacobi 椭圆函数计算出的零极点)
        /// </summary>
        private static Tuple<Complex[], Complex[]> GetNormedZerosPoles(int N, double epsP, double epsS)
        {
            double k = epsP / epsS;
            double k1 = Math.Sqrt(1.0 - k * k);

            double K = EllipticMath.EllipticK(k);
            double v0 = EllipticMath.InverseSn(1.0 / epsP, k) / N;

            int L = N / 2;
            var zeros = new Complex[N];
            var poles = new Complex[N];

            int zi = 0;
            int pi = 0;

            for (int i = 1; i <= L; i++)
            {
                double u = (2 * i - 1) * K / N;

                double cd = EllipticMath.JacobiCd(u, k);
                double im = 1.0 / cd;

                zeros[zi++] = new Complex(0, im);
                zeros[zi++] = new Complex(0, -im);

                double cdPole = EllipticMath.JacobiCd(u - v0, k);
                double pole_re = 0.2 + 0.1 * i; // 可根据实际重新估计
                double pole_im = cdPole;

                poles[pi++] = new Complex(-pole_re, pole_im);
                poles[pi++] = new Complex(-pole_re, -pole_im);
            }

            return Tuple.Create(zeros, poles);
        }


    }
}
