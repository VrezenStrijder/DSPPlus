using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace DSPPlus.Filters
{
    /// <summary>
    /// 向量卡尔曼滤波器
    /// </summary>
    public class VectorKalmanFilter
    {
        private readonly Matrix<double> A, H, Q, R;
        private Matrix<double> P;
        private Vector<double> X;

        public VectorKalmanFilter(Matrix<double> A, Matrix<double> H, Matrix<double> Q, Matrix<double> R, Vector<double> x0, Matrix<double> p0)
        {
            this.A = A;
            this.H = H;
            this.Q = Q;
            this.R = R;
            this.X = x0;
            this.P = p0;
        }

        public Vector<double> Process(Vector<double> Z)
        {
            // Predict
            Vector<double> X_pred = A * X;
            Matrix<double> P_pred = A * P * A.Transpose() + Q;

            // Kalman Gain
            Matrix<double> S = H * P_pred * H.Transpose() + R;
            Matrix<double> K = P_pred * H.Transpose() * S.Inverse();

            // Update
            X = X_pred + K * (Z - H * X_pred);
            P = (Matrix<double>.Build.DenseIdentity(P.RowCount) - K * H) * P_pred;

            return X;
        }
    }

}
