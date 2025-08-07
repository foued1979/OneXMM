using Accord.Math.Optimization;
using System;

namespace OneXMM.Math.Optimizers
{
    using OneXMM.Interfaces;
    using OneXMM.Referential;

    internal static class SmileOptimizer
    {
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

        private class VolObjectiveFunction : NonlinearObjectiveFunction
        {
            public VolObjectiveFunction(DateTime asof, double spot, double fwd, ISmile smile, Dictionary<Option, double> options)
            {
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
        }
    }
}
