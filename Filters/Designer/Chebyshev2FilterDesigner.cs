using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MathNet.Filtering.IIR;
using MathNet.Filtering.TransferFunctions;

namespace DSPPlus.Filters
{

    public static class Chebyshev2FilterDesigner
    {
        public static IFilter DesignFilter(FilterParameter param)
        {
            var freqBand = param.FrequencyBands.FirstOrDefault();
            var type = freqBand.FrequencyType;

            switch (type)
            {
                case FrequencyFilterType.LowPass:
                    return DesignLowPass(param.Order, freqBand.Cutoff1, param.SampleRate, param.StopbandAttenuation ?? 40);
                case FrequencyFilterType.HighPass:
                    return DesignHighPass(param.Order, freqBand.Cutoff1, param.SampleRate, param.StopbandAttenuation ?? 40);
                case FrequencyFilterType.BandPass:
                    return DesignBandPass(param.Order, freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, param.StopbandAttenuation ?? 40);
                case FrequencyFilterType.BandStop:
                    return DesignBandStop(param.Order, freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, param.StopbandAttenuation ?? 40);
                default:
                    return null;
            }
        }

        public static IIRFilter DesignLowPass(int order, double cutoffFreq, double sampleRate, double stopbandAttenuation)
        {
            var (zAnalog, pAnalog) = GetNormedPolesII(order, stopbandAttenuation);
            double gain = 1.0;

            var (zDigital, pDigital, kDigital) = ZpkUtilities.BilinearTransform(zAnalog, pAnalog, gain, sampleRate, cutoffFreq);
            var (b, a) = ZpkUtilities.ZpkToTf(zDigital, pDigital, kDigital);

            return new IIRFilter(new OnlineIirFilter(b.Concat(a).ToArray()));
        }

        public static IIRFilter DesignHighPass(int order, double cutoffFreq, double sampleRate, double stopbandAttenuation)
        {
            // 预变换：频率反转
            var (zAnalog, pAnalog) = GetNormedPolesII(order, stopbandAttenuation);
            zAnalog = zAnalog.Select(z => -1.0 / z).ToArray();
            pAnalog = pAnalog.Select(p => -1.0 / p).ToArray();

            double gain = 1.0;
            var (zDigital, pDigital, kDigital) = ZpkUtilities.BilinearTransform(zAnalog, pAnalog, gain, sampleRate, cutoffFreq);
            var (b, a) = ZpkUtilities.ZpkToTf(zDigital, pDigital, kDigital);

            return new IIRFilter(new OnlineIirFilter(b.Concat(a).ToArray()));
        }

        public static IIRFilter DesignBandPass(int order, double lowCut, double highCut, double sampleRate, double stopbandAttenuation)
        {
            double bw = highCut - lowCut;
            double fc = Math.Sqrt(lowCut * highCut); // 中心频率

            var (zAnalog, pAnalog) = GetNormedPolesII(order, stopbandAttenuation);

            // 类似于模拟域的频率变换：s → (s^2 + ω₀²)/(B*s)
            var zTransformed = zAnalog.SelectMany(z => ZpkUtilities.BandTransform(z, bw, fc)).ToArray();
            var pTransformed = pAnalog.SelectMany(p => ZpkUtilities.BandTransform(p, bw, fc)).ToArray();

            var (zDigital, pDigital, kDigital) = ZpkUtilities.BilinearTransform(zTransformed, pTransformed, 1.0, sampleRate, fc);
            var (b, a) = ZpkUtilities.ZpkToTf(zDigital, pDigital, kDigital);

            return new IIRFilter(new OnlineIirFilter(b.Concat(a).ToArray()));
        }

        public static IIRFilter DesignBandStop(int order, double lowCut, double highCut, double sampleRate, double stopbandAttenuation)
        {
            double bw = highCut - lowCut;
            double fc = Math.Sqrt(lowCut * highCut); // 中心频率

            var (zAnalog, pAnalog) = GetNormedPolesII(order, stopbandAttenuation);

            // 频率变换：s → B*s / (s^2 + ω₀²)
            var zTransformed = zAnalog.SelectMany(z => ZpkUtilities.BandStopTransform(z, bw, fc)).ToArray();
            var pTransformed = pAnalog.SelectMany(p => ZpkUtilities.BandStopTransform(p, bw, fc)).ToArray();

            var (zDigital, pDigital, kDigital) = ZpkUtilities.BilinearTransform(zTransformed, pTransformed, 1.0, sampleRate, fc);
            var (b, a) = ZpkUtilities.ZpkToTf(zDigital, pDigital, kDigital);

            return new IIRFilter(new OnlineIirFilter(b.Concat(a).ToArray()));
        }

        private static (Complex[] Zeros, Complex[] Poles) GetNormedPolesII(int N, double EpsS, double W0 = 1)
        {
            double arcsh(double x) => Math.Log(x + Math.Sqrt(x * x + 1));
            var beta = arcsh(EpsS) / N;
            var sh = Math.Sinh(beta);
            var ch = Math.Cosh(beta);

            var r = N % 2;
            var zeros = new List<Complex>();
            var poles = new List<Complex>();

            if (r == 1)
            {
                poles.Add(-W0 / sh);
            }

            double dth = Math.PI / (2 * N);
            for (int i = r; i < N; i += 2)
            {
                double theta = dth * (i - r + 1);
                var sin = Math.Sin(theta);
                var cos = Math.Cos(theta);

                Complex z = new Complex(-sh * sin, ch * cos);
                double norm = W0 / Complex.Abs(z);
                Complex p = z * norm;
                poles.Add(p);
                poles.Add(Complex.Conjugate(p));

                Complex zZero = new Complex(0, W0 / cos);
                zeros.Add(zZero);
                zeros.Add(Complex.Conjugate(zZero));
            }

            return (zeros.ToArray(), poles.ToArray());
        }
    }
}
