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
                case FilterType.IIR_Butterworth:
                    return CreateIIRButterworth(param);

                case FilterType.FIR_Windowed:
                    return CreateFIRFilter(param);

                case FilterType.FFT:
                    return new FFTFilter(param);

                case FilterType.Kalman:
                    return new KalmanFilter(0.01, 0.1, 0, 1); // 固定参数，实际可拓展

                case FilterType.Chebyshev:
                    return CreateChebyshevFilter(param);

                case FilterType.Elliptic:
                    return CreateEllipticFilter(param);

                case FilterType.Bessel:
                    return BesselFilter.CreateExample(); // TODO: 实现设计方法
                case FilterType.AutoZero:
                    return new AutoZeroFilter((int)param.SampleRate);

                default:
                    throw new NotImplementedException("Unsupported filter type.");
            }
        }

        private static IFilter CreateIIRButterworth(FilterParameter param)
        {
            switch (param.FrequencyType)
            {
                case FrequencyFilterType.LowPass:
                    return IIRFilter.DoLowPass(param.Cutoff1, param.SampleRate, param.Order);
                case FrequencyFilterType.HighPass:
                    return IIRFilter.DoHighPass(param.Cutoff1, param.SampleRate, param.Order);
                case FrequencyFilterType.BandPass:
                    return IIRFilter.DoBandPass(param.Cutoff1, param.Cutoff2.Value, param.SampleRate, param.Order);
                case FrequencyFilterType.BandStop:
                    return IIRFilter.DoBandStop(param.Cutoff1, param.Cutoff2.Value, param.SampleRate, param.Order);
                default:
                    return null;
            }
        }

        private static IFilter CreateFIRFilter(FilterParameter param)
        {
            
            switch (param.FrequencyType)
            {
                case FrequencyFilterType.LowPass:
                    return FIRFilter.DoLowPass(param.Cutoff1, param.SampleRate, param.Order);
                case FrequencyFilterType.HighPass:
                    return FIRFilter.DoHighPass(param.Cutoff1, param.SampleRate, param.Order);
                case FrequencyFilterType.BandPass:
                    return FIRFilter.DoBandPass(param.Cutoff1, param.Cutoff2.Value, param.SampleRate, param.Order);
                case FrequencyFilterType.BandStop:
                    return FIRFilter.DoBandStop(param.Cutoff1, param.Cutoff2.Value, param.SampleRate, param.Order);
                default:
                    return null;
            }
        }

        private static IFilter CreateChebyshevFilter(FilterParameter param)
        {
            return Chebyshev2FilterDesigner.DesignLowPass(param.Cutoff1, param.SampleRate, param.Order);
        }

        private static IFilter CreateEllipticFilter(FilterParameter param)
        {
            return EllipticFilterDesigner.DesignHighPass(param.Cutoff1, param.SampleRate, param.Order);
        }
    }

}
