using Accord.Statistics.Distributions.Univariate;
using System;

namespace OneXMM.Math.Distributions
{
    public sealed class Edgeworth
    {
        private static Func<double, double> H3 = (z) => z * z * z - 3 * z;
        private static Func<double, double> H4 = (z) => { var z2 = z * z; return z2 * z2 - 6 * z2 + 3; };
        private static Func<double, double> H5 = (z) => { var z3 = z * z * z; return z3 * z * z - 10 * z3 + 15 * z; };
        private static Func<double, double> H6 = (z) => { var z2 = z * z; var z4 = z2 * z2; return z4 * z2 - 15 * z4 + 45 * z2 - 15; };
        private static Func<double, double> H7 = (z) => { var z2 = z * z; var z3 = z2 * z; var z5 = z3 * z2; return z5 * z2 - 21 * z5 + 105 * (z3 - z); };
        private static Func<double, double> H9 = (z) => { var z2 = z * z; var z3 = z2 * z; var z5 = z3 * z2; var z7 = z5 * z2; return z7 * z2 - 36 * z7 + 378 * z5 - 1260 * z3 + 945 * z; };

        private readonly NormalDistribution _nd;
        private readonly double _K3;
        private readonly double _K4;
        private readonly double _K6;
        private readonly double _K5;
        private readonly double _K7;
        private readonly double _K9;

        public Edgeworth(double mean, double sigma, double skew, double excessKurtosis, double k5)
        {
            _nd = new NormalDistribution(mean, sigma);
            _K3 = skew / 6.0;
            _K4 = excessKurtosis / 24.0;
            _K6 = skew * skew / 72.0;
            _K5 = k5 / 120.0;
            _K7 = skew * excessKurtosis / 144.0;
            _K9 = skew * skew * skew / 1296.0;
        }

        public double f(double x)
        {
            var phi = _nd.ProbabilityDensityFunction(x);
            return phi * (1 + _K3 * H3(x) + _K4 * H4(x) + _K5 * H5(x) + _K6 * H6(x) + _K7 * H7(x) + _K9 * H9(x));
        }
    }
}
