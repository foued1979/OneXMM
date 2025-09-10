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

        public static bool CalibrateSmile(ref ISmile smile, double[] strikes, double[] targetVols)
        {
            var nelderMead = new NelderMead(new VolObjectiveFunction(smile, strikes, targetVols));
            nelderMead.Convergence = new Accord.Math.Convergence.GeneralConvergence(4)
            {
                MaximumEvaluations = 1500,
                AbsoluteFunctionTolerance = 1E-10,
                AbsoluteParameterTolerance = [1E-12, 1E-12, 1E-12, 1E-12]
            };

            var success = nelderMead.Minimize(smile.GetInitialGuess());
            smile.UpdateParameters(nelderMead.Solution);
            return success;
        }

        public static bool CalibrateSmile(double spot, ref ISmile smile, Dictionary<Option, double> options)
        {
            var nelderMead = new NelderMead(new VolObjectiveFunction(smile.SmileDate, spot, smile, options));
            nelderMead.Convergence = new Accord.Math.Convergence.GeneralConvergence(4)
            {
                MaximumEvaluations = 1500,
                AbsoluteFunctionTolerance = 1E-10,
                AbsoluteParameterTolerance = [1E-12, 1E-12, 1E-12, 1E-12]
            };

            var success = nelderMead.Minimize(smile.GetInitialGuess());
            smile.UpdateParameters(nelderMead.Solution);
            return success;
        }

        public static bool CalibrateSmile(double fwd, ref ISmile smile, CallOption[] options)
        {
            var nelderMead = new NelderMead(new VolObjectiveFunction(smile, options));
            nelderMead.Convergence = new Accord.Math.Convergence.GeneralConvergence(4)
            {
                MaximumEvaluations = 1500,
                AbsoluteFunctionTolerance = 1E-10,
                AbsoluteParameterTolerance = [1E-12, 1E-12, 1E-12, 1E-12]
            };

            var success = nelderMead.Minimize(smile.GetInitialGuess());
            smile.UpdateParameters(nelderMead.Solution);
            return success;
        }

        private class VolObjectiveFunction : NonlinearObjectiveFunction
        {
            public VolObjectiveFunction(DateTime asof, double spot, ISmile smile, Dictionary<Option, double> options)
            {
                NumberOfVariables = smile.GetInitialGuess().Length;
                var smileConstraints = smile.GetSmileConstraints();

                Function = new Func<double[], double>((x) =>
                {
                    if (!smileConstraints.CheckSmileParameters(x)) { return double.PositiveInfinity; }

                    var res = 0.0;
                    smile.UpdateParameters(x);

                    foreach (var option in options)
                    {
                        var err = option.Key.Price(asof, spot, smile.ForwardRef, smile) - option.Value;
                        res += err * err / option.Value;
                    }

                    return res;
                });
            }

            public VolObjectiveFunction(ISmile smile, CallOption[] options)
            {
                NumberOfVariables = smile.GetInitialGuess().Length;
                var smileConstraints = smile.GetSmileConstraints();

                Function = new Func<double[], double>((x) =>
                {
                    if (smileConstraints.CheckSmileParameters(x)) { return double.PositiveInfinity; }

                    var res = 0.0;
                    smile.UpdateParameters(x);

                    foreach (var option in options)
                    {
                        var err = CallPrice(smile.ForwardRef, option.Strike, smile.GetVol(smile.ForwardRef, option.Strike), smile.Expiry) - option.Price;
                        res += err * err / option.Price;
                    }

                    return res;
                });
            }

            public VolObjectiveFunction(ISmile smile, double[] strikes, double[] targetVols)
            {
                NumberOfVariables = smile.GetInitialGuess().Length;
                var smileConstraints = smile.GetSmileConstraints();

                Function = new Func<double[], double>((x) =>
                {
                    if (smileConstraints.CheckSmileParameters(x)) { return double.PositiveInfinity; }

                    var res = 0.0;
                    smile.UpdateParameters(x);

                    for (int i = 0; i < strikes.Length; ++i)
                    {
                        var err = smile.GetVol(smile.ForwardRef, strikes[i]) - targetVols[i];
                        res += err * err;
                    }

                    return res;
                });
            }
        }
    }
}
