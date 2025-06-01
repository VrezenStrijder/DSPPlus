using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public static class FilterUtility
    {
        private static readonly Dictionary<FilterType, HashSet<FrequencyFilterType>> supportMap = new Dictionary<FilterType, HashSet<FrequencyFilterType>>
        {
            { FilterType.Butterworth, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass, FrequencyFilterType.BandPass, FrequencyFilterType.BandStop } },
            { FilterType.Windowed, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass, FrequencyFilterType.BandPass, FrequencyFilterType.BandStop } },
            { FilterType.FFT, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass, FrequencyFilterType.BandPass, FrequencyFilterType.BandStop } },
            { FilterType.Chebyshev, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass, FrequencyFilterType.BandPass, FrequencyFilterType.BandStop } },
            { FilterType.Elliptic, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass, FrequencyFilterType.BandPass, FrequencyFilterType.BandStop } },
            { FilterType.Bessel, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass, FrequencyFilterType.BandPass, FrequencyFilterType.BandStop } },
            { FilterType.ParksMcClellan, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass, FrequencyFilterType.BandPass, FrequencyFilterType.BandStop } },
            { FilterType.Kalman, new HashSet<FrequencyFilterType>() }, // 不支持频率滤波
            { FilterType.SavitzkyGolay, new HashSet<FrequencyFilterType>() }, // 不支持频率滤波
        };

        public static bool IsSupported(FilterType filterType, FrequencyFilterType freqType)
        {
            return supportMap.TryGetValue(filterType, out var supported) && supported.Contains(freqType);
        }
    }
}
