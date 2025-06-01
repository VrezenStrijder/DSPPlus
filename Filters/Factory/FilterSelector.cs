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
            { FilterPurpose.RealTime, FilterType.Butterworth },
            { FilterPurpose.OfflineAnalysis, FilterType.FFT },
            { FilterPurpose.Audio, FilterType.Windowed },
            { FilterPurpose.NoisySignal, FilterType.Kalman },
            { FilterPurpose.Biomedical, FilterType.Chebyshev },
            { FilterPurpose.ControlSystems, FilterType.Bessel },
            { FilterPurpose.Communication, FilterType.Elliptic },
            { FilterPurpose.HighSelectivity, FilterType.Elliptic },
            { FilterPurpose.RippleSensitive, FilterType.Chebyshev },
            { FilterPurpose.RealtimeSmoothing, FilterType.SavitzkyGolay }
        };

        public static IFilter Select(FilterPurpose purpose, FilterParameter parameter)
        {
            // 对于没有指定过滤器类型的情况，针对性处理
            if (purpose == FilterPurpose.AutoZero)
            {
                return new AutoZeroFilter(AutoZeroAlgorithm.LinearDetrend, (int)parameter.SampleRate);
            }

            var filterType = PurposeMap[purpose];
            return FilterFactory.Create(filterType, parameter);
        }
    }

}
