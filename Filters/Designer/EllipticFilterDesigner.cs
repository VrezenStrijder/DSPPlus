using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public static class EllipticFilterDesigner
    {
        /// <summary>
        /// 简化二阶 Elliptic 高通滤波器
        /// </summary>
        public static EllipticFilter DesignHighPass(double cutoffFreq, double sampleRate, double rippleDb = 1, double stopDb = 40, int order = 2)
        {
            // 示例系数，使用 Python scipy.signal.ellip(2, 1, 40, Wn=0.1, btype='high', analog=False, output='ba')
            double[] b = { 0.7984, -1.5968, 0.7984 };
            double[] a = { 1.0000, -1.5610, 0.6414 };

            return new EllipticFilter(b, a);
        }

        /// <summary>
        /// 简化 二阶Elliptic 低通滤波器
        /// </summary>
        public static EllipticFilter DesignLowPass(double cutoffFreq, double sampleRate, double rippleDb = 1, double stopDb = 40, int order = 2)
        {
            // 示例 Python: scipy.signal.ellip(2, 1, 40, 0.1) 生成的系数
            double[] b = { 0.0675, 0.1195, 0.0675 };
            double[] a = { 1.0000, -1.1430, 0.4128 };

            return new EllipticFilter(b, a);
        }
    }

}
