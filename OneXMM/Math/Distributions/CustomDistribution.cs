using Accord.Statistics.Distributions.Univariate;
using MathNet.Numerics.Interpolation;
using System;
using System.Threading.Tasks;

namespace OneXMM.Math.Distributions
{
    using MathNet.Numerics.Integration;
    using OneXMM.Interfaces;
    using OneXMM.Math.Extrapolators;

    public sealed class CustomDistribution
    {
        private const int N = 100;
        private const double EPSILON = 1E-6;
        private static readonly int[] _subXIndices;
        private double[] _subX = [];

        private readonly double[] _x;
        private double[] _pdf;
        private Interpolators.PiecewiseLinearInterpolator<ZeroExtrapolator, ZeroExtrapolator> _interpolator;
        private readonly double _fwd;

        public double[] X => _x;
        public double[] PDF => _pdf;
        public double[] SubX
        {
            get
            {
                if (_subX.Length == 0)
                {
                    _subX = new double[_subXIndices.Length];
                    for (int i = 0; i < _subXIndices.Length; ++i)
                    {
                        _subX[i] = _x[_subXIndices[i]];
                    }
                }
                
                return _subX;
            }
        }
        public double[] StandardizedMoments { get; private set; } = [];

        static CustomDistribution()
        {
            var i0 = (N - 1) / 2;

            if (N % 2 == 0)
            {
                var di = (i0 - 1) / 4;
                var i0_1 = i0 + 1;

                _subXIndices = new int[8];
                
                for (int i = 0; i < 3; i++)
                {
                    _subXIndices[i] = i0 - (3 - i) * di;
                    _subXIndices[i + 5] = i0_1 + (i + 1) * di;
                }

                _subXIndices[3] = i0;
                _subXIndices[4] = i0_1;
            }
            else
            {
                _subXIndices = new int[7];

                var di = (i0 - 1) / 4;
                for (int i = 0; i < 3; i++)
                {
                    _subXIndices[i] = i0 - (3 - i) * di;
                    _subXIndices[i + 4] = i0 + (i + 1) * di;
                }

                _subXIndices[3] = i0;
            }
        }

        public CustomDistribution(ISmile smile)
        {
            var forward = smile.ForwardRef;
            _fwd = forward;

            var sqrtT = System.Math.Sqrt(smile.Expiry);
            var atmVol = smile.GetVol(_fwd, _fwd);
            var vol_sqrtT = atmVol * sqrtT;
            var atmVar = 0.5 * atmVol * atmVol * smile.Expiry;

            _x = new double[N];
            _pdf = new double[N];

            var L = NormalDistribution.Standard.InverseDistributionFunction(1 - EPSILON) * vol_sqrtT;
            var min = -L - 0.5 * vol_sqrtT * vol_sqrtT;
            var max = L - 0.5 * vol_sqrtT * vol_sqrtT;
            var dx = (max - min) / (N - 1);

            var tasks = new Task[N];

            for (int i = 0; i < N; i++)
            {
                var index = i;

                tasks[i] = Task.Run(() =>
                {
                    _x[index] = min + index * dx;
                    double strike = forward * System.Math.Exp(_x[index]);

                    var bump = strike * (System.Math.Exp(1E-4) - 1.0);
                    var one_over_bumpSq = 1.0 / (bump * bump);

                    var strikeUp = strike + bump;
                    var strikeDown = strike - bump;

                    var pUp2 = CallPrice(forward, strikeUp + bump, smile.GetVol(forward, strikeUp + bump), sqrtT);
                    var pUp = CallPrice(forward, strikeUp, smile.GetVol(forward, strikeUp), sqrtT);
                    var p = CallPrice(forward, strike, smile.GetVol(forward, strike), sqrtT);
                    var pDown = CallPrice(forward, strikeDown, smile.GetVol(forward, strikeDown), sqrtT);
                    var pDown2 = CallPrice(forward, strikeDown - bump, smile.GetVol(forward, strikeDown - bump), sqrtT);

                    _pdf[index] = strike * (pUp + pDown - 2 * p) * one_over_bumpSq;
                });
            }

            Task.WaitAll(tasks);

            Norm(_x, ref _pdf);

            _interpolator = new Interpolators.PiecewiseLinearInterpolator<ZeroExtrapolator, ZeroExtrapolator>(_x, _pdf);
        }

        public void ComputeMoments()
        {
            StandardizedMoments = new double[5];
            var mean = GaussLegendreRule.Integrate((x) => x * Pdf(x), -5, 5, 256);

            var tasks = new Task<double>[4];
            for (int i = 0; i < 4; i++)
            {
                tasks[i] = Task.Run(() => GaussLegendreRule.Integrate((x) => System.Math.Pow(x - mean, i + 2) * Pdf(x), -5, 5, 256));
            }

            Task.WaitAll(tasks);

            StandardizedMoments[0] = mean;
            StandardizedMoments[1] = System.Math.Sqrt(tasks[1].Result);
            StandardizedMoments[2] = tasks[2].Result / System.Math.Pow(StandardizedMoments[1], 3);
            StandardizedMoments[3] = tasks[3].Result / System.Math.Pow(StandardizedMoments[1], 4) - 3.0;
            StandardizedMoments[4] = tasks[4].Result / System.Math.Pow(StandardizedMoments[1], 5);
        }

        private static void Norm(double[] d, ref double[] pdf)
        {
            var interpolator = CubicSpline.InterpolateNatural(d, pdf);
            var area = MathNet.Numerics.Integration.GaussKronrodRule.Integrate((x) => interpolator.Interpolate(x), d.First(), d.Last(), out var err, out var norm);

            for (int i = 0; i < pdf.Length; i++) pdf[i] /= area;
        }

        /// <summary>
        /// Gets the density in Ln{K/F).
        /// </summary>
        /// <param name="x">The moneyness, equal to Ln(F/K).</param>
        public double Pdf(double k) => _interpolator.Interpolate(k);

        public double dPdk(double k)
        {
            var index = 0;
            if (k >= _x.Last()) index = N - 1;
            else index = Locate(k);

            if (k.IsCloseTo(_x[index])) return _dpdk(index);

            var prop = (k - _x[index]) / (_x[index + 1] - _x[index]);
            return (1 - prop) * _dpdk(index) + prop * _dpdk(index + 1);
        }

        public double d2Pdk2(double k)
        {
            var index = 0;
            if (k >= _x.Last()) index = N - 1;
            else index = Locate(k);

            if (k.IsCloseTo(_x[index])) return _d2pdk2(index);
            var prop = (k - _x[index]) / (_x[index + 1] - _x[index]);
            return (1 - prop) * _d2pdk2(index) + prop * _d2pdk2(index + 1);
        }

        private int Locate(double k)
        {
            var index = 1;
            while (k > _x[index]) index++;
            return index;
        }

        private double _dpdk(int index)
        {
            if (index == 0) return (-25 * _pdf[0] + 48 * _pdf[1] - 36 * _pdf[2] + 16 * _pdf[3] - 3 * _pdf[4]) / (12.0 * (_x[1] - _x[0]));
            if (index == 1) return (-25 * _pdf[1] + 48 * _pdf[2] - 36 * _pdf[3] + 16 * _pdf[4] - 3 * _pdf[5]) / (12.0 * (_x[1] - _x[0]));
            if (index == N - 2) return (25 * _pdf[N - 2] - 48 * _pdf[N - 3] + 36 * _pdf[N - 4] - 16 * _pdf[N - 5] + 3 * _pdf[N - 6]) / (12.0 * (_x[N - 1] - _x[N - 2]));
            if (index == N - 1) return (25 * _pdf[N - 1] - 48 * _pdf[N - 2] + 36 * _pdf[N - 3] - 16 * _pdf[N - 4] + 3 * _pdf[N - 5]) / (12.0 * (_x[N - 1] - _x[N - 2]));

            return (-_pdf[index + 2] + 8 * _pdf[index + 1] - 8 * _pdf[index - 1] + _pdf[index - 2]) / (12.0 * (_x[index + 1] - _x[index]));
        }

        private double _d2pdk2(int index)
        {
            var dk = 0.0;

            if (index == 0) { dk = _x[1] - _x[0]; return (35 * _pdf[0] - 104 * _pdf[1] + 114 * _pdf[2] - 56 * _pdf[3] + 11 * _pdf[4]) / (12 * dk * dk); }
            if (index == 1) { dk = _x[1] - _x[0]; return (35 * _pdf[1] - 104 * _pdf[2] + 114 * _pdf[3] - 56 * _pdf[4] + 11 * _pdf[5]) / (12 * dk * dk); }
            if (index == N - 2) { dk = _x[N - 1] - _x[N - 2]; return (35 * _pdf[N - 2] - 104 * _pdf[N - 3] + 114 * _pdf[N - 4] - 56 * _pdf[N - 5] + 11 * _pdf[N - 6]) / (12 * dk * dk); }
            if (index == N - 1) { dk = _x[N - 1] - _x[N - 2]; return (35 * _pdf[N - 1] - 104 * _pdf[N - 2] + 114 * _pdf[N - 3] - 56 * _pdf[N - 4] + 11 * _pdf[N - 5]) / (12 * dk * dk); }

            dk = _x[index + 1] - _x[index];
            return (-_pdf[index + 2] + 16 * _pdf[index + 1] - 30 * _pdf[index] + 16 * _pdf[index - 1] - _pdf[index - 2]) / (12 * dk * dk);
        }

        private static Func<double, double, double, double, double> CallPrice = (forward, strike, vol, sqrtT) =>
        {
            var volSqrtT = vol * sqrtT;
            var d1 = System.Math.Log(forward / strike) / volSqrtT + 0.5 * volSqrtT;
            var d2 = d1 - volSqrtT;

            return forward * NormalDistribution.Standard.DistributionFunction(d1) - strike * NormalDistribution.Standard.DistributionFunction(d2);
        };
    }
}
