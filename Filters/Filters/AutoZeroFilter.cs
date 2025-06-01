using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public class AutoZeroFilter : IFilter
    {
        private int windowSize;
        private AutoZeroAlgorithm algorithm;

        public AutoZeroFilter(AutoZeroAlgorithm algorithm, int window)
        {
            this.algorithm = algorithm;
            windowSize = window;
        }

        public double Process(double input)
        {
            return input;
        }

        public double[] ProcessBatch(double[] inputSignal)
        {
            if (algorithm == AutoZeroAlgorithm.LinearDetrend)
            {
                return AutoZeroUtility.LinearDetrend(inputSignal);
            }
            return AutoZeroUtility.MovingAverage(inputSignal, windowSize);
        }
    }
}
