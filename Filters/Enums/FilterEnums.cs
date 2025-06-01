using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public enum FilterCategory
    {
        [Description("无限冲击响应(IIR)")]
        IIR,
        [Description("有限冲击响应(FIR)")]
        FIR
    }

    public enum FilterType
    {
        [Description("巴特沃斯滤波器")]
        Butterworth = 0,

        [Description("切比雪夫滤波器")]
        Chebyshev = 1,

        [Description("卡尔曼滤波器")]
        Kalman = 2,

        [Description("贝塞尔滤波器")]
        Bessel = 3,

        [Description("椭圆滤波器")]
        Elliptic = 4,

        /* 以上为 IIR 滤波器 */

        [Description("窗函数滤波器")]
        Windowed = 5,

        [Description("FFT频域滤波器")]
        FFT = 6,

        [Description("Savitzky-Golay滤波器")]
        SavitzkyGolay = 7,

        [Description("Parks-McClellan等波纹滤波器")]
        ParksMcClellan = 8

        /* 以上为 FIR 滤波器 */
    }


    /// <summary>
    /// 滤波器用途
    /// </summary>
    public enum FilterPurpose
    {
        AutoZero,           // 自动零点
        RealTime,           // 嵌入式实时信号处理
        OfflineAnalysis,    // 离线数据分析,精度优先
        Audio,              // 保证相位线性,音频处理
        NoisySignal,        // 噪声鲁棒性
        Biomedical,         // ECG,EEG,需要线性相位
        Communication,      // 滤除特定频段
        ControlSystems,     // 相位延迟敏感
        HighSelectivity,    // 高频隔离精度需求高
        RippleSensitive,    // 对纹波敏感
        RealtimeSmoothing,  // 实时平滑
    }


    /// <summary>
    /// 频率响应类型
    /// </summary>
    public enum FrequencyFilterType
    {
        [Description("低通滤波器")]
        LowPass,
        [Description("高通滤波器")]
        HighPass,
        [Description("带通滤波器")]
        BandPass,
        [Description("带阻滤波器")]
        BandStop
    }

    public enum AutoZeroAlgorithm
    {
        MovingAverage,      // 移动平均法
        LinearDetrend       // 线性去趋势法
    }

}
