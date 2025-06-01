using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public class AutoZeroUtility
    {
        /// <summary>
        /// 滑动平均法
        /// (局部，适合处理周期性/实时数据)
        /// </summary>
        public static double[] MovingAverage(double[] input, int windowSize)
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

        /// <summary>
        /// 线性校正法(最小二乘法)
        /// (全局，不适合处理周期性/实时数据)
        /// </summary>
        public static double[] LinearDetrend(double[] input)
        {
            int n = input.Length;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                sumX += i;
                sumY += input[i];
                sumXY += i * input[i];
                sumX2 += i * i;
            }

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            // 计算每个点的基线值并减去
            double[] output = new double[n];
            for (int i = 0; i < n; i++)
            {
                double baseline = slope * i + intercept;
                output[i] = input[i] - baseline;
            }

            return output;
        }
    }
}
