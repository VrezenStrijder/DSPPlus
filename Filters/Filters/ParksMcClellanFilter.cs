using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public class ParksMcClellanFilter : IFilter
    {
        private readonly FIRFilter fir;

        public ParksMcClellanFilter(double[] coeffs)
        {
            fir = new FIRFilter(coeffs);
        }

        public double Process(double input) => fir.Process(input);

        public double[] ProcessBatch(double[] input) => fir.ProcessBatch(input);

        public double[] GetCoefficients() => fir.GetCoefficients();

        /// <summary>
        public static ParksMcClellanFilter Design(FilterParameter param)
        {
            var spec = ParksMcClellanUtils.ConvertFilterParameter(param);

            var designer = new ParksMcClellanDesigner(param.Order, spec.Bands, spec.Desired, spec.Weights);
            var coeffs = designer.Design();

            return new ParksMcClellanFilter(coeffs);
        }

    }
}
