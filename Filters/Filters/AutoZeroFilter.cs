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

        public AutoZeroFilter(int window)
        {
            windowSize = window;
        }

        public double Process(double input)
        {
            return input;
        }

        public double[] ProcessBatch(double[] inputSignal)
        {
            return FilterUtility.AutoZero(inputSignal, windowSize);
        }
    }
}
