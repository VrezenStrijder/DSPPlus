using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    /// <summary>
    /// 卡尔曼滤波器
    /// 
    /// 状态预测：
    /// [
    ///     x_k = A \cdot x_{k-1} +B \cdot u_k + w_k
    /// ]
    ///
    /// 测量更新：
    /// [
    ///     z_k = H \cdot x_k + v_k
    /// ]
    ///
    /// 其中：
    /// (x_k)：状态向量
    /// (z_k)：测量值
    /// (A)：状态转移矩阵
    /// (B)：控制输入矩阵
    /// (H)：测量矩阵
    /// (w_k)：过程噪声
    /// (v_k)：测量噪声
    ///
    /// </summary>
    public class KalmanFilter : IFilter
    {
        private double Q; // 过程噪声
        private double R; // 测量噪声
        private double x; // 估计值
        private double P; // 估计误差

        public KalmanFilter(double processNoise, double measurementNoise, double initialEstimate, double initialErrorCovariance)
        {
            Q = processNoise;
            R = measurementNoise;
            x = initialEstimate;
            P = initialErrorCovariance;
        }

        public KalmanFilter(FilterParameter param)
        {
            Q = param.ProcessNoise ?? 0.01;
            R = param.MeasurementNoise ?? 0.1;
            x = param.InitialEstimate ?? 0;
            P = param.InitialErrorCovariance ?? 1;
        }

        public double Process(double measurement)
        {
            P += Q;
            double K = P / (P + R);
            x = x + K * (measurement - x);
            P = (1 - K) * P;
            return x;
        }

        public double[] ProcessBatch(double[] inputSignal)
        {
            double[] output = new double[inputSignal.Length];
            for (int i = 0; i < inputSignal.Length; i++)
            {
                output[i] = Process(inputSignal[i]);
            }
            return output;
        }
    }

}
