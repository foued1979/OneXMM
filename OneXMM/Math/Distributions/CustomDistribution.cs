using System;
using Accord.Statistics.Distributions.Univariate;

namespace OneXMM.Math.Distributions
{
    using OneXMM.Interfaces;

    public sealed class CustomDistribution
    {
        private static readonly double[] _x;
        private static readonly NormalDistribution _snd;
        private double[] _pdf;

        static CustomDistribution()
        {
            _x = new double[99];
            var snd = new NormalDistribution(0, 1);

            for (int i = 0; i < 99; i++)
            {
                _x[i] = snd.QuantileDensityFunction(0.01 * (i + 1));
            }

            _snd = snd;
        }

        public CustomDistribution(double forward, ISmile smile)
        {
            var sqrtT = System.Math.Sqrt(smile.Expiry);
            _pdf = new double[99];

            for (int i = 0; i < _x.Length; i++)
            {
                var index = i;
                Task.Run(() => {
                    var strike = smile.GetStrikeFromMoneyness(forward, _x[index]);
                    var bump = strike * 1E-5;
                    var pUp = CallPrice(forward, strike + bump, smile.GetVol(forward, strike + bump), sqrtT);
                    var p = CallPrice(forward, strike, smile.GetVol(forward, strike), sqrtT);
                    var pDown = CallPrice(forward, strike - bump, smile.GetVol(forward, strike - bump), sqrtT);

                    _pdf[index] = (pUp + pDown - 2 * p) / (bump * bump);
                });
            }
        }

        private Func<double, double, double, double, double> CallPrice = (forward, strike, vol, sqrtT) =>
        {
            var volSqrtT = vol * sqrtT;
            var d1 = System.Math.Log(forward / strike) / volSqrtT + 0.5 * volSqrtT;
            var d2 = d1 - volSqrtT;

            return forward * _snd.DistributionFunction(d1) - strike * _snd.DistributionFunction(d2);
        };
    }
}
