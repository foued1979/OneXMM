using Accord.Statistics.Distributions.Univariate;
using System;

namespace OneXMM.Math.Distributions
{
    using OneXMM.Interfaces;
    using OneXMM.Math.Extrapolators;
    using OneXMM.Math.Interpolators;
    using System.Threading.Tasks;

    public sealed class CustomDistribution
    {
        private static readonly double[] _p = [1E-16, 1E-12, 1E-8, 1E-4, 0.01, 0.05, 0.1, 0.125, 0.15, 0.175, 0.2, 0.22, 0.24, 0.26, 0.28, 0.3, 0.3125, 0.325, 0.3375, 0.35, 0.3625, 0.375, 0.3875, 0.4, 0.41, 0.42, 0.43, 0.44, 0.45, 0.455, 0.46, 0.465, 0.47, 0.475, 0.48, 0.485, 0.49, 0.495, 0.5 ];
        private readonly double[] _x;
        private double[] _pdf;
        private PiecewiseLinearInterpolator<ZeroExtrapolator, ZeroExtrapolator> _interpolator;

        public double[] X => _x;
        public double[] PDF => _pdf;

        public CustomDistribution(double forward, ISmile smile)
        {
            _x = new double[_p.Length * 2 - 1];
            _pdf = new double[_x.Length];

            for (int i = 0; i < _p.Length; i++)
            {
                _x[i] = NormalDistribution.Standard.InverseDistributionFunction(_p[i]);
                if (i < _p.Length - 1) _x[2 * (_p.Length - 1) - i] = -_x[i];
            }

            var sqrtT = System.Math.Sqrt(smile.Expiry);
            var atmVol = smile.GetVol(forward, forward);
            var atmVar = 0.5 * atmVol * atmVol * smile.Expiry;
            var bump = forward * 1E-4;

            var tasks = new Task[_x.Length];

            for (int i = 0; i < _x.Length; i++)
            {
                var index = i;

                tasks[i] = Task.Run(() =>
                {
                    double strike = forward * System.Math.Exp(_x[index] * atmVol * sqrtT - atmVar);
                    var volGuess = smile.GetVol(forward, strike);
                    strike = forward * System.Math.Exp(_x[index] * volGuess * sqrtT - 0.5 * volGuess * volGuess * smile.Expiry);

                    _x[index] = smile.GetMoneyness(forward, strike);

                    var strikeUp = strike + bump;
                    var strikeDown = System.Math.Max(0.0, strike - bump);

                    var pUp = CallPrice(forward, strikeUp, smile.GetVol(forward, strikeUp), sqrtT);
                    var p = CallPrice(forward, strike, smile.GetVol(forward, strike), sqrtT);
                    var pDown = CallPrice(forward, strikeDown, smile.GetVol(forward, strikeDown), sqrtT);

                    _pdf[index] = (pUp + pDown - 2 * p) / (bump * bump);

                    if (_pdf[index] < 0)
                    {
                        if (index == 0 || index == _x.Length - 1)
                        {
                            _pdf[index] = 0;
                        }
                        else
                        {
                            _pdf[index] = 0.5 * (_pdf[index + 1] + _pdf[index - 1]);
                        }
                    }
                });
            }

            Task.WaitAll(tasks);

            Norm(_x, ref _pdf);
            _interpolator = new PiecewiseLinearInterpolator<ZeroExtrapolator, ZeroExtrapolator>(_x, _pdf);
        }

        public void Rebase(out ISmile newSmile)
        {

        }

        private static void Norm(double[] d, ref double[] pdf)
        {
            var interpolator = new PiecewiseLinearInterpolator<ZeroExtrapolator, ZeroExtrapolator>(d, pdf);
            var integral = new Accord.Math.Integration.NonAdaptiveGaussKronrod()
            {
                Function = new Func<double, double>((x) => interpolator.Interpolate(x)),
                Range = new Accord.DoubleRange(0.9 * d.First(), 1.1 * d.Last()),
                ToleranceAbsolute = 1E-12,
                ToleranceRelative = 1E-8
            };

            integral.Compute();

            for (int i = 0; i < pdf.Length; i++) pdf[i] /= integral.Area;
        }

        public double Pdf(double x) => _interpolator.Interpolate(x);

        private Func<double, double, double, double, double> CallPrice = (forward, strike, vol, sqrtT) =>
        {
            var volSqrtT = vol * sqrtT;
            var d1 = System.Math.Log(forward / strike) / volSqrtT + 0.5 * volSqrtT;
            var d2 = d1 - volSqrtT;

            return forward * NormalDistribution.Standard.DistributionFunction(d1) - strike * NormalDistribution.Standard.DistributionFunction(d2);
        };
    }
}
