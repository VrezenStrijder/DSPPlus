using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public class ConvertedFilterSpec
    {
        public double[] Bands { get; set; }
        public double[] Desired { get; set; }
        public double[] Weights { get; set; }
    }

    public static class ParksMcClellanUtils
    {
        public static ConvertedFilterSpec ConvertFilterParameter(FilterParameter param)
        {
            double nyquist = param.SampleRate / 2;

            double rippleDb = param.PassbandRipple ?? 0.5;
            double stopDb = param.StopbandAttenuation ?? 60;

            double passWeight = 1.0 / ParksMcClellanDesigner.DbToPassbandWeight(rippleDb);
            double stopWeight = 1.0 / ParksMcClellanDesigner.DbToStopbandWeight(stopDb);

            var sortedBands = param.FrequencyBands.OrderBy(b => b.Cutoff1).ToList();

            var bandEdges = new List<double>();
            var desired = new List<double>();
            var weights = new List<double>();

            foreach (var band in sortedBands)
            {
                double f1 = band.Cutoff1 / param.SampleRate;
                double f2 = (band.Cutoff2 ?? (band.Cutoff1 + 0.05)) / param.SampleRate;

                switch (band.FrequencyType)
                {
                    case FrequencyFilterType.LowPass:
                        bandEdges.Add(0.0);
                        bandEdges.Add(f1);
                        desired.Add(1.0);
                        weights.Add(passWeight);

                        bandEdges.Add(f1 + 0.05);
                        bandEdges.Add(0.5);
                        desired.Add(0.0);
                        weights.Add(stopWeight);
                        break;

                    case FrequencyFilterType.HighPass:
                        bandEdges.Add(0.0);
                        bandEdges.Add(f1 - 0.05);
                        desired.Add(0.0);
                        weights.Add(stopWeight);

                        bandEdges.Add(f1);
                        bandEdges.Add(0.5);
                        desired.Add(1.0);
                        weights.Add(passWeight);
                        break;

                    case FrequencyFilterType.BandPass:
                        bandEdges.Add(f1 - 0.02);
                        bandEdges.Add(f1);
                        desired.Add(0.0);
                        weights.Add(stopWeight);

                        bandEdges.Add(f1);
                        bandEdges.Add(f2);
                        desired.Add(1.0);
                        weights.Add(passWeight);

                        bandEdges.Add(f2);
                        bandEdges.Add(f2 + 0.02);
                        desired.Add(0.0);
                        weights.Add(stopWeight);
                        break;

                    case FrequencyFilterType.BandStop:
                        bandEdges.Add(f1 - 0.02);
                        bandEdges.Add(f1);
                        desired.Add(1.0);
                        weights.Add(passWeight);

                        bandEdges.Add(f1);
                        bandEdges.Add(f2);
                        desired.Add(0.0);
                        weights.Add(stopWeight);

                        bandEdges.Add(f2);
                        bandEdges.Add(f2 + 0.02);
                        desired.Add(1.0);
                        weights.Add(passWeight);
                        break;

                    default:
                        throw new NotSupportedException($"未支持的频带类型: {band.FrequencyType}");
                }
            }

            return new ConvertedFilterSpec
            {
                Bands = bandEdges.ToArray(),
                Desired = desired.ToArray(),
                Weights = weights.ToArray(),
            };
        }
    }

    public static class ParksMcClellanFilterGenerator
    {

        public static double[] DesignBasic(FrequencyFilterType type, double sampleRate, int order, double cutoff1, double? cutoff2 = null, double passbandRipple = 0.01, double stopbandAttenuation = 60)
        {
            var freqBand = new FrequencyBand(type, cutoff1, cutoff2);

            var param = new FilterParameter
            {
                FrequencyBands = new List<FrequencyBand> { freqBand },
                SampleRate = sampleRate,
                Order = order,
                PassbandRipple = passbandRipple,
                StopbandAttenuation = stopbandAttenuation
            };

            var filter = ParksMcClellanFilter.Design(param);
            return filter.GetCoefficients();
        }

        /// <summary>
        /// 直接传入归一化频率/目标响应/权重
        /// </summary>
        public static double[] DesignCustomFreqBand(int order, double[] freqs, double[] desired, double[] weights)
        {
            var designer = new ParksMcClellanDesigner(order, freqs, desired, weights);
            return designer.Design();
        }

    }
}
