using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DSPPlus.Filters
{
    public static class ZpkUtilities
    {
        public static (int Div, int Mod) GetDivMod(this int x, int y) => (x / y, x % y);

        public static double Log10(this int x) => Math.Log10(x);

        public static int Log10Int(this int x) => (int)Math.Log10(x);
        /// <summary>
        /// 双线性变换(将模拟零点/极点转换为数字域)
        /// </summary>
        public static (Complex[] Z, Complex[] P, double K) BilinearTransform(Complex[] zAnalog, Complex[] pAnalog, double gain, double sampleRate, double preWarpedCutoff)
        {
            var fs2 = 2.0 * sampleRate;

            Complex Bilinear(Complex s) => (1.0 + s / fs2) / (1.0 - s / fs2);

            var zDigital = zAnalog.Select(Bilinear).ToArray();
            var pDigital = pAnalog.Select(Bilinear).ToArray();

            // 获取补偿
            int nZeros = zDigital.Length;
            int nPoles = pDigital.Length;
            int n = Math.Max(nZeros, nPoles);

            double gainFactor = gain * Math.Pow(fs2, nPoles - nZeros);

            return (zDigital, pDigital, gainFactor);
        }

        public static IEnumerable<Complex> BandTransform(Complex s, double bw, double w0)
        {
            // s → (s^2 + w0^2) / (bw * s)
            Complex[] result = new Complex[2];
            Complex a = 1;
            Complex b = bw;
            Complex c = -w0 * w0;

            Complex discriminant = Complex.Sqrt(s * s - 4 * c);
            return new[] { (s + discriminant) / (2 * a), (s - discriminant) / (2 * a) };
        }

        public static IEnumerable<Complex> BandStopTransform(Complex s, double bw, double w0)
        {
            // s → bw*s / (s^2 + w0^2)
            Complex j = Complex.ImaginaryOne;
            Complex s2 = s * s;
            Complex denom = s2 + w0 * w0;

            return new[] { (bw * s) / denom };
        }


        /// <summary>
        /// 将零点/极点增益转换为多项式传递函数系数
        /// </summary>
        public static (double[] b, double[] a) ZpkToTf(Complex[] z, Complex[] p, double k)
        {
            var b = Poly(z);
            var a = Poly(p);

            for (int i = 0; i < b.Length; i++)
            {
                b[i] *= k;
            }
            return (b.Select(x => x.Real).ToArray(), a.Select(x => x.Real).ToArray());
        }


        private static Complex[] Poly(Complex[] roots)
        {
            var coeffs = new List<Complex> { 1.0 };

            foreach (var root in roots)
            {
                var newCoeffs = new Complex[coeffs.Count + 1];
                for (int i = 0; i < coeffs.Count; i++)
                {
                    newCoeffs[i] += -root * coeffs[i];
                    newCoeffs[i + 1] += coeffs[i];
                }
                coeffs = newCoeffs.ToList();
            }

            return coeffs.ToArray();
        }
    }

}
