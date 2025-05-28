using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.IntegralTransforms;
using Utility;

namespace DSPPlus.Filters
{
    [Obsolete]
    public class FrequencyFilter : InfoCallbackBase
    {
        public FrequencyFilter(InfoCallbackHandler callbackHandler = null)
        {
            if (callbackHandler != null)
            {
                InfoCallback += callbackHandler;
            }
        }

        /// <summary>
        /// 低通滤波器
        /// </summary>
        public double[] LowPassFilter(double[] signal, double samplingRate, double cutoffFrequency)
        {
            return ApplyFrequencyFilter(signal, samplingRate, freq => freq > cutoffFrequency);
        }

        /// <summary>
        /// 高通滤波器
        /// </summary>
        public double[] HighPassFilter(double[] signal, double samplingRate, double cutoffFrequency)
        {
            return ApplyFrequencyFilter(signal, samplingRate, freq => freq < cutoffFrequency);
        }

        /// <summary>
        /// 带通滤波器
        /// </summary>
        public double[] BandPassFilter(double[] signal, double samplingRate, double lowCutoff, double highCutoff)
        {
            return ApplyFrequencyFilter(signal, samplingRate, freq => freq < lowCutoff || freq > highCutoff);
        }

        /// <summary>
        /// 阻通滤波器 (陷波器)
        /// </summary>
        public double[] BandStopFilter(double[] signal, double samplingRate, double lowCutoff, double highCutoff)
        {
            return ApplyFrequencyFilter(signal, samplingRate, freq => freq >= lowCutoff && freq <= highCutoff);
        }

        /// <summary>
        /// 通用频域滤波器
        /// </summary>
        private double[] ApplyFrequencyFilter(double[] signal, double samplingRate, Func<double, bool> filterCondition)
        {
            int n = signal.Length;

            FireInfoCallback(new InfoCallbackEventArgs("准备进行滤波转换..."));
            // 转换到频域
            Complex[] frequencyDomain = new Complex[n];
            for (int i = 0; i < n; i++)
            {
                frequencyDomain[i] = new Complex(signal[i], 0);
            }
            Fourier.Forward(frequencyDomain, FourierOptions.Matlab);

            // 计算频率分辨率
            double frequencyResolution = samplingRate / n;

            double[] frequencyBins = new double[n];
            for (int i = 0; i < n; i++)
            {
                frequencyBins[i] = (i <= n / 2) ? i * frequencyResolution : (i - n) * frequencyResolution;
            }

            // 应用滤波器条件
            for (int i = 0; i < n; i++)
            {
                if (filterCondition(frequencyBins[i]))
                {
                    frequencyDomain[i] = Complex.Zero; // 滤除不需要的频率成分
                }

                ShowFeedback(n, i);
            }

            // 转换回时域
            FireInfoCallback(new InfoCallbackEventArgs("正在对滤波信号进行反变换..."));
            Fourier.Inverse(frequencyDomain, FourierOptions.Matlab);

            double[] filteredSignal = new double[n];
            for (int i = 0; i < n; i++)
            {
                filteredSignal[i] = frequencyDomain[i].Real; // 取实部
            }

            FireInfoCallback(new InfoCallbackEventArgs("滤波处理完成."));

            return filteredSignal;
        }

        private void ShowFeedback(long len, int n)
        {
            if (n % 100000 == 0 || (n >= len - 1))
            {
                string info = $"正在进行滤波处理,进度: {Math.Round((n * 1d / len) * 100, 2)}%";
                if (n == len - 1)
                {
                    info = $"滤波转换结束";
                }
                FireInfoCallback(new InfoCallbackEventArgs(info));
            }
        }

        /// <summary>
        /// 对输入信号进行高通滤波
        /// </summary>
        /// <param name="signal">输入时域信号</param>
        /// <param name="samplingRate">采样频率 (Hz)</param>
        /// <param name="cutoffFrequency">截止频率 (高通滤波器的最低频率，单位 Hz)</param>
        /// <returns>滤波后的信号</returns>
        //public static double[] HighPassFilter2(double[] signal, double samplingRate, double cutoffFrequency)
        //{
        //    // 将时域信号转换为频域信号 (FFT)
        //    int n = signal.Length;
        //    Complex[] frequencyDomain = new Complex[n];
        //    for (int i = 0; i < n; i++)
        //    {
        //        frequencyDomain[i] = new Complex(signal[i], 0);
        //    }
        //    Fourier.Forward(frequencyDomain, FourierOptions.Matlab);

        //    // 计算频率分辨率 (每个频率分量对应的频率间隔)
        //    double frequencyResolution = samplingRate / n;

        //    // 应用高通滤波器：将低于截止频率的频率分量置零
        //    for (int i = 0; i < n; i++)
        //    {
        //        double frequency = i * frequencyResolution;
        //        if (frequency < cutoffFrequency)
        //        {
        //            frequencyDomain[i] = Complex.Zero; // 阻塞低频信号
        //        }
        //    }

        //    // 将频域信号转换回时域信号 (IFFT)
        //    Fourier.Inverse(frequencyDomain, FourierOptions.Matlab);
        //    double[] filteredSignal = new double[n];
        //    for (int i = 0; i < n; i++)
        //    {
        //        filteredSignal[i] = frequencyDomain[i].Real; // 取实部
        //    }

        //    return filteredSignal;
        //}
    }


}
