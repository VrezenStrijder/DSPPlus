using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Filtering.IIR;
using MathNet.Numerics;

namespace DSPPlus.Filters
{
    public class ChebyshevFilter : IFilter
    {
        private readonly IIRFilter internalFilter;

        public ChebyshevFilter(double[] b, double[] a)
        {
            internalFilter = new IIRFilter(b, a);
        }

        public ChebyshevFilter(FilterParameter param)
        {
            var freqBand = param.FrequencyBands.FirstOrDefault();
            var type = freqBand.FrequencyType;
            switch (type)
            {
                case FrequencyFilterType.LowPass:
                    internalFilter= Chebyshev2FilterDesigner.DesignLowPass(param.Order, freqBand.Cutoff1, param.SampleRate, param.StopbandAttenuation ?? 40);
                    break;
                case FrequencyFilterType.HighPass:
                    internalFilter = Chebyshev2FilterDesigner.DesignHighPass(param.Order, freqBand.Cutoff1, param.SampleRate, param.StopbandAttenuation ?? 40);
                    break;
                case FrequencyFilterType.BandPass:
                    internalFilter = Chebyshev2FilterDesigner.DesignBandPass(param.Order, freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, param.StopbandAttenuation ?? 40);
                    break;
                case FrequencyFilterType.BandStop:
                    internalFilter = Chebyshev2FilterDesigner.DesignBandStop(param.Order, freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, param.StopbandAttenuation ?? 40);
                    break;
                default:
                    throw new Exception("未支持的频率响应类型.");
            }
        }

        public double Process(double input) => internalFilter.Process(input);

        public double[] ProcessBatch(double[] inputSignal) => internalFilter.ProcessBatch(inputSignal);
    }
}
