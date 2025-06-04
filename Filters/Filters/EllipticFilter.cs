using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{

    public class EllipticFilter : IFilter
    {
        private readonly IIRFilter internalFilter;

        public EllipticFilter(double[] b, double[] a)
        {
            internalFilter = new IIRFilter(b, a);
        }

        public EllipticFilter(FilterParameter param)
        {
            var freqBand = param.FrequencyBands.FirstOrDefault();
            var type = freqBand.FrequencyType;

            double ripple = param.PassbandRipple ?? 1;
            double stopband = param.StopbandAttenuation ?? 60;

            switch (type)
            {
                case FrequencyFilterType.LowPass:
                    internalFilter = EllipticFilterDesigner.DesignLowPass(param.Order, freqBand.Cutoff1, param.SampleRate, ripple, stopband);
                    break;
                case FrequencyFilterType.HighPass:
                    internalFilter = EllipticFilterDesigner.DesignHighPass(param.Order, freqBand.Cutoff1, param.SampleRate, ripple, stopband);
                    break;
                case FrequencyFilterType.BandPass:
                    internalFilter = EllipticFilterDesigner.DesignBandPass(param.Order, freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, ripple, stopband);
                    break;
                case FrequencyFilterType.BandStop:
                    internalFilter = EllipticFilterDesigner.DesignBandStop(param.Order, freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, ripple, stopband);
                    break;
                default:
                    throw new Exception("Unsupported filter type.");
            }
        }


        public double Process(double input) => internalFilter.Process(input);

        public double[] ProcessBatch(double[] inputSignal) => internalFilter.ProcessBatch(inputSignal);
    }


}
