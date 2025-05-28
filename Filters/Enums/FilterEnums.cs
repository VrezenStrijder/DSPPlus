using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    /// <summary>
    /// 滤波器类型
    /// </summary>
    public enum FilterType
    {
        IIR_Butterworth,
        FIR_Windowed,
        FFT,
        Kalman,
        Chebyshev,
        Elliptic,
        Bessel,
        AutoZero
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
        RippleSensitive     // 对纹波敏感
    }


    /// <summary>
    /// 频率响应类型
    /// </summary>
    public enum FrequencyFilterType
    {
        LowPass,
        HighPass,
        BandPass,
        BandStop
    }

}
