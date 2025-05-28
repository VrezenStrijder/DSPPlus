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

        public double Process(double input) => internalFilter.Process(input);

        public double[] ProcessBatch(double[] inputSignal) => internalFilter.ProcessBatch(inputSignal);
    }


}
