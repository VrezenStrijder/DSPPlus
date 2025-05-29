using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public class FilterParameter
    {
        public double SampleRate { get; set; }

        public FrequencyFilterType FrequencyType { get; set; }

        public double Cutoff1 { get; set; }

        public double? Cutoff2 { get; set; } = null;

        public int Order { get; set; } = 2;
    }

}
