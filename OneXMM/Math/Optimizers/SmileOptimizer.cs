using Accord.Math.Optimization;
using System;

namespace OneXMM.Math.Optimizers
{
    using Accord.Statistics.Distributions.Univariate;
    using OneXMM.Interfaces;
    using OneXMM.Pricing;

    internal static class SmileOptimizer
    {
        public static Func<double, double, double, double, double> CallPrice = (fwd, strike, vol, expiry) =>
        {
            var vol_sqrtT = vol * System.Math.Sqrt(expiry);
            var d1 = System.Math.Log(fwd / strike) / vol_sqrtT + 0.5 * vol_sqrtT;
            var d2 = d1 - vol_sqrtT;

            return fwd * NormalDistribution.Standard.DistributionFunction(d1) - strike * NormalDistribution.Standard.DistributionFunction(d2);
        };

        public class CallOption
        {
            public double Strike { get; }
            public double Price { get; }

            public CallOption(double forward, double strike, double vol, double expiry)
            {
                Strike = strike;
                Price = CallPrice(forward, strike, vol, expiry);
            }
        }

        public static bool CalibrateSmile(double spot, double fwd, ref ISmile smile, Dictionary<Option, double> options)
        {
            var nelderMead = new NelderMead(new VolObjectiveFunction(smile.SmileDate, spot, fwd, smile, options));
            nelderMead.Convergence = new Accord.Math.Convergence.GeneralConvergence(4)
            {
                MaximumEvaluations = 1500,
                AbsoluteFunctionTolerance = 1E-10,
                AbsoluteParameterTolerance = [1E-12, 1E-12, 1E-12, 1E-12]
            };

            var initialGuess = new double[4];

            initialGuess[0] = 0.5;
            initialGuess[1] = 0.35;
            initialGuess[2] = 0.72;
            initialGuess[3] = 0.2;

            var success = nelderMead.Minimize(initialGuess);
            smile.UpdateParameters(nelderMead.Solution);
            return success;
        }

        public static bool CalibrateSmile(double fwd, ref ISmile smile, CallOption[] options)
        {
            var nelderMead = new NelderMead(new VolObjectiveFunction(fwd, smile, options));
            nelderMead.Convergence = new Accord.Math.Convergence.GeneralConvergence(4)
            {
                MaximumEvaluations = 1500,
                AbsoluteFunctionTolerance = 1E-10,
                AbsoluteParameterTolerance = [1E-12, 1E-12, 1E-12, 1E-12]
            };

            var initialGuess = new double[4];

            initialGuess[0] = 0.5;
            initialGuess[1] = 0.35;
            initialGuess[2] = 0.72;
            initialGuess[3] = 0.2;

            var success = nelderMead.Minimize(initialGuess);
            smile.UpdateParameters(nelderMead.Solution);
            return success;
        }

        private class VolObjectiveFunction : NonlinearObjectiveFunction
        {
            public VolObjectiveFunction(DateTime asof, double spot, double fwd, ISmile smile, Dictionary<Option, double> options)
            {
                NumberOfVariables = 4;
                Function = new Func<double[], double>((x) =>
                {
                    if (x[3] < -1 || x[3] > 1 || x[2] < 0 || x[2] > 1) { return double.PositiveInfinity; }

                    var res = 0.0;
                    smile.UpdateParameters(x);

                    foreach (var option in options)
                    {
                        var err = option.Key.Price(asof, spot, fwd, smile) - option.Value;
                        res += err * err / option.Value;
                    }

                    return res;
                });
            }

            public VolObjectiveFunction(double fwd, ISmile smile, CallOption[] options)
            {
                NumberOfVariables = 4;
                Function = new Func<double[], double>((x) =>
                {
                    if (x[3] < -1 || x[3] > 1 || x[2] < 0 || x[2] > 1) { return double.PositiveInfinity; }

                    var res = 0.0;
                    smile.UpdateParameters(x);

                    foreach (var option in options)
                    {
                        var err = CallPrice(fwd, option.Strike, smile.GetVol(fwd, option.Strike), smile.Expiry) - option.Price;
                        res += err * err / option.Price;
                    }

                    return res;
                });
            }
        }
    }
}
