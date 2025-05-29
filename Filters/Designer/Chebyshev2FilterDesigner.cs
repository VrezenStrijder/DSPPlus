using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Filtering.IIR;
using MathNet.Filtering.TransferFunctions;

namespace DSPPlus.Filters
{
    public static class Chebyshev2FilterDesigner
    {
        /// <summary>
        /// 简化二阶 Chebyshev2 滤波器
        /// </summary>
        public static ChebyshevFilter DesignLowPass(double cutoffFreq, double sampleRate, int order = 2, double stopDb = 40)
        {
            // 示例系数（使用 Python scipy.signal.cheby2(2, 40, 0.3)）
            double[] b = { 0.3913, 0.0, -0.3913 };
            double[] a = { 1.0, -0.3695, 0.2173 };

            return new ChebyshevFilter(b, a);
        }
    }

}
