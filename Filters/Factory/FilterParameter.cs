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

        public List<FrequencyBand> FrequencyBands { get; set; } = new List<FrequencyBand>();

        public int Order { get; set; } = 2;

        /// <summary>
        /// 窗口大小
        /// (用于 Savitzky-Golay 滤波器)
        /// </summary>
        public int WindowSize { get; set; }

        /// <summary>
        /// 阻带衰减(dB) 
        /// (用于Chebyshev II/Elliptic/ParksMcClellan 滤波器)
        /// </summary>
        public double? StopbandAttenuation { get; set; }

        /// <summary>
        /// 通带纹波(db)
        /// 在通带(允许信号通过的频率范围)内,滤波器增益的最大波动量
        ///(用于 Elliptic/ParksMcClellan 滤波器)
        /// </summary>
        public double? PassbandRipple { get; set; }

        /// <summary>
        /// 中心频率(Hz)
        /// (用于Bessel 的 BandPass/BandStop模式)
        /// </summary>
        public double? CenterFreq { get; set; }

        /// <summary>
        /// 带宽(Hz)
        /// (用于Bessel 的 BandPass/BandStop模式)
        /// </summary>
        public double? Bandwidth { get; set; }

        /// <summary>
        /// 过程噪声
        /// (用于 Kalman 滤波器)
        /// </summary>
        public double? ProcessNoise { get; set; }

        /// <summary>
        /// 测量噪声
        /// (用于 Kalman 滤波器)
        /// </summary>
        public double? MeasurementNoise { get; set; }

        /// <summary>
        /// 初始估计值
        /// (用于 Kalman 滤波器)
        /// </summary>
        public double? InitialEstimate { get; set; }

        /// <summary>
        /// 初始预估误差
        /// (用于 Kalman 滤波器)
        /// </summary>
        public double? InitialErrorCovariance { get; set; }

    }


    public class FrequencyBand
    {

        public FrequencyBand()
        {
        }

        public FrequencyBand(FrequencyFilterType frequencyType)
        {
            FrequencyType = frequencyType;
        }

        public FrequencyBand(FrequencyFilterType frequencyType, double cutoff1, double? cutoff2 = null) : this(frequencyType)
        {
            Cutoff1 = cutoff1;
            Cutoff2 = cutoff2;
        }

        public FrequencyFilterType FrequencyType { get; set; }

        public double Cutoff1 { get; set; }

        public double? Cutoff2 { get; set; } = null;
    }
}
