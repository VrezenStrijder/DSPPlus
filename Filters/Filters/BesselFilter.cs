using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MathNet.Filtering.IIR;

namespace DSPPlus.Filters
{

    public class BesselFilter: IFilter
    {
        private readonly IIRFilter internalFilter;

        public BesselFilter(FilterParameter param)
        {
            var freqBand = param.FrequencyBands.FirstOrDefault();
            var type = freqBand.FrequencyType;

            switch (type)
            {
                case FrequencyFilterType.LowPass:
                    internalFilter = BesselFilterDesigner.DesignLowPass(param.Order, freqBand.Cutoff1, param.SampleRate) ;
                    break;
                case FrequencyFilterType.HighPass:
                    internalFilter = BesselFilterDesigner.DesignHighPass(param.Order, freqBand.Cutoff1, param.SampleRate);
                    break;
                case FrequencyFilterType.BandPass:
                    internalFilter = BesselFilterDesigner.DesignBandPass(param.Order, param.CenterFreq.Value, param.Bandwidth.Value, param.SampleRate);
                    break;
                case FrequencyFilterType.BandStop:
                    internalFilter = BesselFilterDesigner.DesignBandStop(param.Order, param.CenterFreq.Value, param.Bandwidth.Value, param.SampleRate);
                    break;
                default:
                    throw new Exception("Unsupported filter type.");
            }
        }


        public double Process(double input) => internalFilter.Process(input);

        public double[] ProcessBatch(double[] inputSignal) => internalFilter.ProcessBatch(inputSignal);

    }


}
