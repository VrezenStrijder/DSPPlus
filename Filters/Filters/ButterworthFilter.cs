using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public class ButterworthFilter : IFilter
    {
        private readonly IIRFilter internalFilter;

        private ButterworthFilter(IIRFilter iir)
        {
            internalFilter = iir;
        }

        public double Process(double input) => internalFilter.Process(input);

        public double[] ProcessBatch(double[] inputSignal) => internalFilter.ProcessBatch(inputSignal);

        public static ButterworthFilter CreateHighPass(double cutoffFreq, double sampleRate, int order)
        {
            return new ButterworthFilter(IIRFilter.DoHighPass(cutoffFreq, sampleRate, order));
        }

        public static ButterworthFilter CreateLowPass(double cutoffFreq, double sampleRate, int order)
        {
            return new ButterworthFilter(IIRFilter.DoLowPass(cutoffFreq, sampleRate, order));
        }

        public static ButterworthFilter CreateBandPass(double cutoffFreqLow, double cutoffFreqHigh, double sampleRate, int order)
        {
            return new ButterworthFilter(IIRFilter.DoBandPass(cutoffFreqLow, cutoffFreqHigh, sampleRate, order));
        }

        public static ButterworthFilter CreateBandStop(double cutoffFreqLow, double cutoffFreqHigh, double sampleRate, int order)
        {
            return new ButterworthFilter(IIRFilter.DoBandStop(cutoffFreqLow, cutoffFreqHigh, sampleRate, order));
        }
    }
}
