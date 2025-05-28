using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public interface IFilter
    {
        double Process(double input);

        double[] ProcessBatch(double[] inputSignal);


    }

}
