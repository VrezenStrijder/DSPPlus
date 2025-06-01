using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public static class FilterFactory
    {

        public static IFilter Create(FilterType type, FilterParameter param)
        {
            switch (type)
            {
                case FilterType.Butterworth:
                    return CreateIIRButterworth(param);

                case FilterType.Windowed:
                    return CreateFIRFilter(param);

                case FilterType.FFT:
                    return new FFTFilter(param);

                case FilterType.Kalman:
                    return new KalmanFilter(param);

                case FilterType.Chebyshev:
                    return CreateChebyshevFilter(param);

                case FilterType.Elliptic:
                    return CreateEllipticFilter(param);

                case FilterType.Bessel:
                    return CreateBesselFilter(param);

                case FilterType.SavitzkyGolay:
                    return new SavitzkyGolayFilter(param);

                case FilterType.ParksMcClellan:
                    return CreateParksMcClellanFilter(param);

                default:
                    throw new NotImplementedException("未支持的滤波器类型.");
            }
        }

        private static IFilter CreateIIRButterworth(FilterParameter param)
        {
            var freqBand = param.FrequencyBands.FirstOrDefault();
            switch (freqBand.FrequencyType)
            {
                case FrequencyFilterType.LowPass:
                    return IIRFilter.DoLowPass(freqBand.Cutoff1, param.SampleRate, param.Order);
                case FrequencyFilterType.HighPass:
                    return IIRFilter.DoHighPass(freqBand.Cutoff1, param.SampleRate, param.Order);
                case FrequencyFilterType.BandPass:
                    return IIRFilter.DoBandPass(freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, param.Order);
                case FrequencyFilterType.BandStop:
                    return IIRFilter.DoBandStop(freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, param.Order);
                default:
                    return null;
            }
        }

        private static IFilter CreateFIRFilter(FilterParameter param)
        {
            var freqBand = param.FrequencyBands.FirstOrDefault();
            switch (freqBand.FrequencyType)
            {
                case FrequencyFilterType.LowPass:
                    return FIRFilter.DoLowPass(freqBand.Cutoff1, param.SampleRate, param.Order);
                case FrequencyFilterType.HighPass:
                    return FIRFilter.DoHighPass(freqBand.Cutoff1, param.SampleRate, param.Order);
                case FrequencyFilterType.BandPass:
                    return FIRFilter.DoBandPass(freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, param.Order);
                case FrequencyFilterType.BandStop:
                    return FIRFilter.DoBandStop(freqBand.Cutoff1, freqBand.Cutoff2.Value, param.SampleRate, param.Order);
                default:
                    return null;
            }
        }

        private static IFilter CreateBesselFilter(FilterParameter param)
        {
            return BesselFilterDesigner.DesignFilter(param);
        }

        private static IFilter CreateChebyshevFilter(FilterParameter param)
        {
            return Chebyshev2FilterDesigner.DesignFilter(param);
        }

        private static IFilter CreateEllipticFilter(FilterParameter param)
        {
            return EllipticFilterDesigner.DesignFilter(param);
        }

        private static IFilter CreateParksMcClellanFilter(FilterParameter param)
        {
            return ParksMcClellanFilter.Design(param);
        }

    }
}
