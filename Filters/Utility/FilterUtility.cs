using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public static class FilterUtility
    {

        public static double[] AutoZero(double[] input, int windowSize)
        {
            double[] output = new double[input.Length];
            double sum = 0;

            for (int i = 0; i < input.Length; i++)
            {
                sum += input[i];

                if (i >= windowSize)
                {
                    sum -= input[i - windowSize];
                    output[i] = input[i] - sum / windowSize;
                }
                else
                {
                    output[i] = input[i] - sum / (i + 1);  // 初始阶段用当前平均
                }
            }

            return output;
        }

        private static readonly Dictionary<FilterType, HashSet<FrequencyFilterType>> supportMap = new Dictionary<FilterType, HashSet<FrequencyFilterType>>
        {
            { FilterType.IIR_Butterworth, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass, FrequencyFilterType.BandPass, FrequencyFilterType.BandStop } },
            { FilterType.FIR_Windowed, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass, FrequencyFilterType.BandPass, FrequencyFilterType.BandStop } },
            { FilterType.FFT, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass, FrequencyFilterType.BandPass, FrequencyFilterType.BandStop } },
            { FilterType.Kalman, new HashSet<FrequencyFilterType>() }, // 不支持频率滤波
            { FilterType.Chebyshev, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass } },
            { FilterType.Elliptic, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass } },
            { FilterType.Bessel, new HashSet<FrequencyFilterType> { FrequencyFilterType.LowPass, FrequencyFilterType.HighPass } }
        };

        public static bool IsSupported(FilterType filterType, FrequencyFilterType freqType)
        {
            return supportMap.TryGetValue(filterType, out var supported) && supported.Contains(freqType);
        }
    }
}
