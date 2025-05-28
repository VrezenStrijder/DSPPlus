using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public class FilterSelector
    {
        private static readonly Dictionary<FilterPurpose, FilterType> PurposeMap = new Dictionary<FilterPurpose, FilterType>
        {
            { FilterPurpose.RealTime, FilterType.IIR_Butterworth },
            { FilterPurpose.OfflineAnalysis, FilterType.FFT },
            { FilterPurpose.Audio, FilterType.FIR_Windowed },
            { FilterPurpose.NoisySignal, FilterType.Kalman },
            { FilterPurpose.Biomedical, FilterType.Chebyshev },
            { FilterPurpose.ControlSystems, FilterType.Bessel },
            { FilterPurpose.Communication, FilterType.Elliptic },
            { FilterPurpose.HighSelectivity, FilterType.Elliptic },
            { FilterPurpose.RippleSensitive, FilterType.Chebyshev },
            { FilterPurpose.AutoZero, FilterType.AutoZero },
        };

        public static IFilter Select(FilterPurpose purpose, FilterParameter parameter)
        {
            var filterType = PurposeMap[purpose];
            return FilterFactory.Create(filterType, parameter);
        }
    }

}
