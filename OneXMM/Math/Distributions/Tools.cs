using System;

namespace OneXMM.Math.Distributions
{
    public static class Tools
    {
        public static double[] ComputeMoments(double[] returns)
        {
            var res = new double[5];
            double mean = 0.0, meanSq = 0.0, sigma = 0.0, skew = 0.0, excessK = 0.0, kk = 0.0;

            for (int i = 0; i < returns.Length; ++i) mean += returns[i];
            mean /= returns.Length;
            meanSq = mean * mean;

            for (int i = 0; i < returns.Length; ++i)
            {
                var m = returns[i] * returns[i];
                sigma += m - meanSq;

                m *= returns[i];
                skew += m;

                m *= returns[i];
                excessK += m;

                m *= returns[i];
                kk += m;
            }

            sigma = System.Math.Sqrt(sigma / (returns.Length - 1));

            var std = System.Math.Pow(sigma, 3);
            skew /= returns.Length * std;

            std *= sigma;
            excessK = excessK / (returns.Length * std) - 3.0;

            std *= sigma;
            kk /= returns.Length * std;

            return [mean, sigma, skew, excessK, kk];
        }
    }
}
