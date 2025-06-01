using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.IntegralTransforms;

namespace DSPPlus.Filters
{
    public class FFTFilter : IFilter
    {
        private readonly Func<double, bool> frequencyMask;
        private readonly FilterParameter parameter;

        public FFTFilter(FilterParameter param)
        {
            this.parameter = param;
            var freqBand = param.FrequencyBands.FirstOrDefault();


            switch (freqBand.FrequencyType)
            {
                case FrequencyFilterType.LowPass:
                    frequencyMask = freq => freq > freqBand.Cutoff1;
                    break;

                case FrequencyFilterType.HighPass:
                    frequencyMask = freq => freq < freqBand.Cutoff1;
                    break;

                case FrequencyFilterType.BandPass:
                    if (freqBand.Cutoff2.HasValue)
                    {
                        frequencyMask = freq => freq < freqBand.Cutoff1 || freq > freqBand.Cutoff2.Value;
                    }
                    break;

                case FrequencyFilterType.BandStop:
                    if (freqBand.Cutoff2.HasValue)
                    {
                        frequencyMask = freq => freq >= freqBand.Cutoff1 && freq <= freqBand.Cutoff2.Value;
                    }
                    break;
            }
        }


        public double Process(double input)
        {
            throw new NotSupportedException("FFTFilter 不支持单点处理，请使用 ProcessBatch");
        }

        public double[] ProcessBatch(double[] inputSignal)
        {
            int n = inputSignal.Length;
            Complex[] freqDomain = new Complex[n];
            for (int i = 0; i < n; i++)
            {
                freqDomain[i] = new Complex(inputSignal[i], 0);
            }

            Fourier.Forward(freqDomain, FourierOptions.Matlab);
            // 频率分辨率
            double freqResolution = parameter.SampleRate / n;

            // 频率预计算
            for (int i = 0; i < n; i++)
            {
                double freq = (i <= n / 2) ? i * freqResolution : (i - n) * freqResolution;
                if (frequencyMask(Math.Abs(freq)))
                {
                    freqDomain[i] = Complex.Zero;
                }
            }

            Fourier.Inverse(freqDomain, FourierOptions.Matlab);
            double[] output = new double[n];
            for (int i = 0; i < n; i++)
            {
                output[i] = freqDomain[i].Real;
            }
            return output;
        }
    }

}
