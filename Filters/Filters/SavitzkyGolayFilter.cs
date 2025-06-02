using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    /// <summary>
    /// Savitzky-Golay滤波器
    /// (平滑 FIR 滤波器)
    /// </summary>
    public class SavitzkyGolayFilter : IFilter
    {
        private readonly double[] coeffs;
        private readonly double[] buffer;

        public SavitzkyGolayFilter(int windowSize, int polyOrder)
        {
            if (windowSize % 2 == 0)
            {
                throw new ArgumentException("窗口大小必须是奇数.");
            }
            if (polyOrder >= windowSize)
            {
                throw new ArgumentException("Poly order 必须小于窗口大小.");
            }
            coeffs = ComputeCoefficients(windowSize, polyOrder);
            buffer = new double[windowSize];
        }

        public SavitzkyGolayFilter(FilterParameter param) : this(param.WindowSize, param.Order)
        {
        }


        public double Process(double input)
        {
            for (int i = buffer.Length - 1; i > 0; i--)
            {
                buffer[i] = buffer[i - 1];
            }
            buffer[0] = input;

            return buffer.Zip(coeffs, (x, c) => x * c).Sum();
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

        private double[] ComputeCoefficients(int window, int polyOrder)
        {
            int m = (window - 1) / 2;
            var A = new double[window, polyOrder + 1];
            for (int i = -m; i <= m; i++)
            {
                for (int j = 0; j <= polyOrder; j++)
                {
                    A[i + m, j] = Math.Pow(i, j);
                }
            }

            var AT = MathNet.Numerics.LinearAlgebra.Double.Matrix.Build.DenseOfArray(A).Transpose();
            var ATA = AT * AT.Transpose();
            var ATAInv = ATA.Inverse();
            var coeffs = ATAInv * AT;

            return coeffs.Row(0).ToArray();
        }
    }

}
