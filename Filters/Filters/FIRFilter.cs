using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    /// <summary>
    /// FIR 滤波器
    /// (适用于相位敏感,计算资源充足的场景)
    /// </summary>
    public class FIRFilter: IFilter
    {
        private double[] coefficients;
        private double[] buffer;
        private int bufferIndex;

        public FIRFilter(double[] filterCoefficients)
        {
            coefficients = filterCoefficients;
            buffer = new double[coefficients.Length];
            bufferIndex = 0;
        }

        public double Process(double input)
        {
            // 将新样本添加到缓冲区
            buffer[bufferIndex] = input;
            bufferIndex = (bufferIndex + 1) % buffer.Length;

            // 计算输出
            double output = 0;
            int index;

            for (int i = 0; i < coefficients.Length; i++)
            {
                index = (bufferIndex - i + buffer.Length) % buffer.Length;
                output += coefficients[i] * buffer[index];
            }

            return output;
        }

        public double[] ProcessBatch(double[] inputSignal)
        {
            double[] output = new double[inputSignal.Length];

            // 重置滤波器状态
            Array.Clear(buffer, 0, buffer.Length);
            bufferIndex = 0;

            for (int i = 0; i < inputSignal.Length; i++)
            {
                output[i] = Process(inputSignal[i]);
            }

            return output;
        }

        /// <summary>
        /// FIR低通滤波器
        /// </summary>
        /// <param name="cutoffFreq"></param>
        /// <param name="sampleRate"></param>
        /// <param name="order">阶数(滤波器系数的数量减1，表示需要存储的过去输入样本数量)</param>
        /// <returns></returns>
        public static FIRFilter DoLowPass(double cutoffFreq, double sampleRate, int order)
        {
            if (order % 2 == 0)
            {
                order++; // 确保阶数为奇数，以实现线性相位
            }
            double[] h = new double[order];
            double omega = 2.0 * Math.PI * cutoffFreq / sampleRate;
            int m = order / 2;

            for (int n = 0; n < order; n++)
            {
                if (n == m)
                {
                    h[n] = omega / Math.PI;
                }
                else
                {
                    h[n] = Math.Sin(omega * (n - m)) / (Math.PI * (n - m));
                }
                // 应用Hamming窗
                h[n] *= 0.54 - 0.46 * Math.Cos(2.0 * Math.PI * n / (order - 1));
            }

            return new FIRFilter(h);
        }

        /// <summary>
        /// FIR高通滤波器
        /// </summary>
        /// <param name="cutoffFreq"></param>
        /// <param name="sampleRate"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static FIRFilter DoHighPass(double cutoffFreq, double sampleRate, int order)
        {
            if (order % 2 == 0)
            {
                order++;
            }
            double[] h = new double[order];
            double omega = 2.0 * Math.PI * cutoffFreq / sampleRate;
            int m = order / 2;

            for (int n = 0; n < order; n++)
            {
                if (n == m)
                {
                    h[n] = 1.0 - omega / Math.PI;
                }
                else
                {
                    h[n] = -Math.Sin(omega * (n - m)) / (Math.PI * (n - m));
                }
                // 应用Hamming窗
                h[n] *= 0.54 - 0.46 * Math.Cos(2.0 * Math.PI * n / (order - 1));
            }

            return new FIRFilter(h);
        }

        /// <summary>
        /// FIR带通滤波器
        /// </summary>
        /// <param name="lowCutoffFreq"></param>
        /// <param name="highCutoffFreq"></param>
        /// <param name="sampleRate"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static FIRFilter DoBandPass(double lowCutoffFreq, double highCutoffFreq, double sampleRate, int order)
        {
            if (order % 2 == 0)
            {
                order++;
            }
            double[] h = new double[order];
            double omega1 = 2.0 * Math.PI * lowCutoffFreq / sampleRate;
            double omega2 = 2.0 * Math.PI * highCutoffFreq / sampleRate;
            int m = order / 2;

            for (int n = 0; n < order; n++)
            {
                if (n == m)
                {
                    h[n] = (omega2 - omega1) / Math.PI;
                }
                else
                {
                    h[n] = (Math.Sin(omega2 * (n - m)) - Math.Sin(omega1 * (n - m))) / (Math.PI * (n - m));
                }
                // 应用Blackman窗
                h[n] *= 0.42 - 0.5 * Math.Cos(2.0 * Math.PI * n / (order - 1)) + 0.08 * Math.Cos(4.0 * Math.PI * n / (order - 1));
            }

            return new FIRFilter(h);
        }

        /// <summary>
        /// FIR阻带滤波器
        /// </summary>
        /// <param name="lowCutoffFreq"></param>
        /// <param name="highCutoffFreq"></param>
        /// <param name="sampleRate"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static FIRFilter DoBandStop(double lowCutoffFreq, double highCutoffFreq, double sampleRate, int order)
        {
            if (order % 2 == 0)
            {
                order++;
            }
            double[] h = new double[order];
            double omega1 = 2.0 * Math.PI * lowCutoffFreq / sampleRate;
            double omega2 = 2.0 * Math.PI * highCutoffFreq / sampleRate;
            int m = order / 2;

            for (int n = 0; n < order; n++)
            {
                if (n == m)
                {
                    h[n] = 1.0 - (omega2 - omega1) / Math.PI;
                }
                else
                {
                    h[n] = (Math.Sin(omega1 * (n - m)) - Math.Sin(omega2 * (n - m))) / (Math.PI * (n - m));
                }
                // 应用Blackman窗
                h[n] *= 0.42 - 0.5 * Math.Cos(2.0 * Math.PI * n / (order - 1)) + 0.08 * Math.Cos(4.0 * Math.PI * n / (order - 1));
            }

            return new FIRFilter(h);
        }
    }

}

