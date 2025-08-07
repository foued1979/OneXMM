using Accord.Math.Decompositions;
using System;

namespace OneXMM.Pricing
{
    public static class VolTools
    {
        public static double FindCorrelatedATMVol(double[] atmVols, double[][] spotPrices, int interval)
        {
            if (atmVols.Length != spotPrices.GetLength(0) - 1) throw new Exception("There should be 1 more array of prices for the asset to evaluate.");
            GetReturns(spotPrices, interval, out var S, out var R);

            var svd = new SingularValueDecomposition(S);
            var reg = svd.Solve(R);

            var res = 0.0;
            var errSq = 0.0;

            for (int i = 0; i < R.Length; ++i)
            {
                var err = 0.0;

                for (int j = 0; j < S.GetLength(1); ++j)
                {
                    err += S[i, j] * reg[j];
                }

                err -= R[i];
                errSq += err * err;
            }

            var std = System.Math.Sqrt(errSq) / (R.Length - 1);

            for (int i = 0; i < reg.Count(); ++i)
            {
                res += System.Math.Pow(atmVols[i] * reg[i], 2);
            }

            return System.Math.Sqrt(res) + std;
        }

        private static void GetReturns(double[][] prices, int interval, out double[,] refReturns, out double[] targetReturns)
        {
            var tasks = new Task[prices.GetLength(0)];
            var returns = new double[prices.GetLength(0)][];

            for (int i = 0; i < tasks.Length; ++i)
            {
                var index = i;

                tasks[i] = Task.Run(() =>
                {
                    var size = System.Math.DivRem(prices[index].Length - 1, interval).Quotient;
                    var returnsForAsset = new double[size];
                    var cIndex = size - 1;

                    for (int j = prices[index].Length - 1 - interval; j >= 0; j -= interval, --cIndex)
                    {
                        returnsForAsset[cIndex] = prices[index][j + interval] / prices[index][j] - 1.0;
                    }

                    returns[index] = returnsForAsset;
                });
            }

            Task.WaitAll(tasks);

            var minSize = int.MaxValue;
            for (int i = 0; i < tasks.Length; ++i) if (returns[i].Length < minSize) minSize = returns[i].Length;
            for (int i = 0; i < tasks.Length; ++i)
            {
                if (returns[i].Length != minSize)
                {
                    var selectedReturns = new double[minSize];
                    Array.Copy(returns[i], returns[i].Length - minSize, selectedReturns, 0, minSize);
                    returns[i] = selectedReturns;
                }
            }

            refReturns = Matrix<double>.Build.DenseOfRows(returns.Take(prices.GetLength(0) - 1)).Transpose();
            targetReturns = Vector<double>.Build.Dense(returns.Last());
        }
    }
}
