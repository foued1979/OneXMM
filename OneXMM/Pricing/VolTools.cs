using Accord.Math.Decompositions;
using OneXMM.Math.Tools;
using System;

namespace OneXMM.Pricing
{
    public static class VolTools
    {
        public static double FindCorrelatedATMVol(double[] atmVols, RollingPriceUpdate[] refSpotPrices, RollingPriceUpdate correlatedPrices, int frequency, int size)
        {
            if (atmVols.Length != refSpotPrices.Length) throw new Exception("There should be 1 more array of prices for the asset to evaluate.");
            var svdArgs = GetReturns(refSpotPrices, correlatedPrices, frequency, size);

            var svd = new SingularValueDecomposition(svdArgs.Item1);
            var reg = svd.Solve(svdArgs.Item2);

            var res = 0.0;
            var errSq = 0.0;

            for (int i = 0; i < svdArgs.Item2.Length; ++i)
            {
                var err = 0.0;

                for (int j = 0; j < svdArgs.Item1.GetLength(1); ++j)
                {
                    err += svdArgs.Item1[i, j] * reg[j];
                }

                err -= svdArgs.Item2[i];
                errSq += err * err;
            }

            var std = System.Math.Sqrt(errSq) / (svdArgs.Item2.Length - 1);

            for (int i = 0; i < reg.Count(); ++i)
            {
                res += System.Math.Pow(atmVols[i] * reg[i], 2);
            }

            return System.Math.Sqrt(res) + std;
        }

        private static (double[,], double[]) GetReturns(RollingPriceUpdate[] refPrices, RollingPriceUpdate correlatedPrices, int frequency, int size)
        {
            var n = refPrices.Length;

            var tasks = new Task<double[]>[n + 1];
            var refReturns = new double[size, n];

            for (int i = 0; i < n; ++i)
            {
                var index = i;
                tasks[i] = Task.Run(() => refPrices[index].ExtractReturns(frequency, size));
            }

            tasks[n] = Task.Run(() => correlatedPrices.ExtractReturns(frequency, size));

            Task.WaitAll(tasks);

            for (int i = 0; i < n; ++i)
            {
                var index = i;
                Task.Run(() =>
                {
                    var ret = tasks[index].Result;

                    for (int j = 0; j < size; ++j)
                    {
                        refReturns[j, index] = ret[j];
                    }
                });
            }

            return (refReturns, tasks[n].Result);
        }
    }
}
