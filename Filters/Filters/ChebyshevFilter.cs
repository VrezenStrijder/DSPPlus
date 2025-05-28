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

        public double Process(double input) => internalFilter.Process(input);

        public double[] ProcessBatch(double[] inputSignal) => internalFilter.ProcessBatch(inputSignal);
    }
}
