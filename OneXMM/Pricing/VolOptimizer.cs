using System;

namespace OneXMM.Pricing
{
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.Optimization;
    using OneXMM.Referential;

    internal static class VolOptimizer
    {
        public static MinimizationResult CalibrateSmile(double spot, double fwd, SABR smile, SortedDictionary<Option, double> options)
        {
            var simplex = new NelderMeadSimplex(1E-5, 1500);
            var initialGuess = new DenseVector(4);

            initialGuess[0] = 0.6;
            initialGuess[1] = 0.3;
            initialGuess[2] = 0.72;
            initialGuess[3] = 0.0;

            var initialBump = new DenseVector(4);

            initialBump[0] = 0.1;
            initialBump[1] = 0.01;
            initialBump[2] = 0.01;
            initialBump[3] = 0.005;

            return simplex.FindMinimum(new VolObjectiveFunction(smile.SmileDate, spot, fwd, smile, options, initialGuess), initialGuess, initialBump);
        }

        private class VolObjectiveFunction : IObjectiveFunction
        {
            private readonly DateTime _asof;
            private readonly double _spot;
            private readonly double _fwd;
            private readonly SABR _smile;
            private readonly SortedDictionary<Option, double> _options;
            private readonly Vector<double> _initGuess;

            public Vector<double> Point { get; private set; }

            public double Value { get; private set; }

            public bool IsGradientSupported => false;

            public Vector<double> Gradient { get; }

            public bool IsHessianSupported => false;

            public Matrix<double> Hessian => throw new NotImplementedException();

            public VolObjectiveFunction(DateTime asof, double spot, double fwd, SABR smile, SortedDictionary<Option, double> options, Vector<double> initialGuess)
            {
                _asof = asof;
                _spot = spot;
                _fwd = fwd;
                _smile = smile;
                _options = options;
                _initGuess = initialGuess;
            }

            public IObjectiveFunction CreateNew()
            {
                return new VolObjectiveFunction(_asof, _spot, _fwd, _smile, _options, _initGuess);
            }

            public void EvaluateAt(Vector<double> point)
            {
                Value = 0;
                Point = point;

                if (point[3] < -1 || point[3] > 1 || point[2] < 0 || point[2] > 1) { Value = double.PositiveInfinity; return; }

                _smile.WithParameters(point[0], point[1], point[2], point[3]);

                foreach (var option in _options)
                {
                    var err = GetError(_asof, option, _spot, _fwd, _smile);
                    Value += err * err;
                }
            }

            public IObjectiveFunction Fork()
            {
                throw new NotImplementedException();
            }

            private static Vector<double> ComputeGradient(DateTime asof, double spot, double fwd, SABR smile, SortedDictionary<Option, double> options, Vector<double> initialGuess)
            {
                var res = new DenseVector(initialGuess.Count);

                for (int i = 0; i < res.Count; ++i)
                {
                    var x0 = initialGuess[i];
                    var bump = 0.0005 * x0;

                    var totalErrUp = 0.0;
                    initialGuess[i] = x0 + bump;
                    smile.WithParameters(initialGuess[0], initialGuess[1], initialGuess[2], initialGuess[3]);

                    foreach (var option in options)
                    {
                        var err = GetError(asof, option, spot, fwd, smile);
                        totalErrUp += err * err;
                    }

                    var totalErrDn = 0.0;
                    initialGuess[i] = x0 - bump;
                    smile.WithParameters(initialGuess[0], initialGuess[1], initialGuess[2], initialGuess[3]);

                    foreach (var option in options)
                    {
                        var err = GetError(asof, option, spot, fwd, smile);
                        totalErrDn += err * err;
                    }

                    res[i] = 0.5 * (totalErrUp - totalErrDn) / bump;
                }

                return res;
            }

            private static double GetError(DateTime asof, KeyValuePair<Option, double> option, double spot, double fwd, SABR smile)
            {
                return option.Key.Price(asof, spot, fwd, smile) - option.Value;
            }
        }
    }
}
