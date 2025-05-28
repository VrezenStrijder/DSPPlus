using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public class BesselFilter : IFilter
    {
        private readonly IIRFilter internalFilter;

        public BesselFilter(double[] b, double[] a)
        {
            internalFilter = new IIRFilter(b, a);
        }

        public double Process(double input) => internalFilter.Process(input);

        public double[] ProcessBatch(double[] inputSignal) => internalFilter.ProcessBatch(inputSignal);

        public static BesselFilter FromCoefficients(double[] b, double[] a)
        {
            return new BesselFilter(b, a);
        }

        /// <summary>
        /// 示例：固定系数 Bessel 低通
        /// </summary>
        /// <returns></returns>
        public static BesselFilter CreateExample()
        {
            double[] b = { 0.01809893, 0.03619786, 0.01809893 };
            double[] a = { 1.0, -1.76004188, 0.80200555 };
            return new BesselFilter(b, a);
        }
    }

}
