using System;

namespace OneXMM.Math.Models
{
    using OneXMM.Interfaces;
    using OneXMM.Math.Distributions;
    using OneXMM.Math.Extrapolators;
    using OneXMM.Math.Interpolators;
    using OneXMM.Math.Models.Smiles;
    using OneXMM.Math.Optimizers;

    public sealed class FokkerPlanck
    {
        private const double dVol = 1E-2;
        private static readonly double one_over_dVol = 1.0 / dVol;
        private static readonly double one_over_dVolSq = one_over_dVol * one_over_dVol;

        private readonly double _spot;
        private readonly ICurve _curve;

        private ISmile _smile;
        private CustomDistribution _customDistribution;

        private double _expiry;
        private readonly double _dt;

        private record VolPoint(double Strike, double Volatility);

        public ISmile Smile => _smile.Copy();
        public double[] StandardizedMoments => _customDistribution.StandardizedMoments;

        public FokkerPlanck(double spot, ISmile smile, ICurve fwdCurve, double dt) 
        {
            _spot = spot;
            _smile = smile.Copy();
            _curve = fwdCurve;

            _expiry = smile.Expiry;
            _dt = dt;
            _customDistribution = new CustomDistribution(fwdCurve.GetForward(spot, _expiry), smile);
        }

        // We solve the Fokker-Planck equation for 1 time interval:
        // dp/dt = -d(mu * p)/dx + 0.5 * d2(sigma^2 * p)/dx2
        public void SolveOneStep()
        {
            var tasks = new Task[_customDistribution.X.Length];
            var fwd = _curve.GetForward(_spot, _expiry);
            var one_over_fwd = 1.0 / fwd;
            var atm = _smile.GetVol(fwd, fwd);
            var pdfs = _customDistribution.PDF;
            var dp = new double[_customDistribution.X.Length];

            for (int i = 0; i < _customDistribution.X.Length; i++)
            {
                var index = i;

                tasks[i] = Task.Run(() =>
                {
                    var k = _customDistribution.X[index];
                    var pdf = pdfs[index];

                    var strike = fwd * System.Math.Exp(k);
                    var dk = 1.0E-4;

                    var dpdx = _customDistribution.dPdk(k);
                    var d2pdx2 = _customDistribution.d2Pdk2(k);

                    // 0.5 * d2(sigma^2 p)/dx2 = 0.5 * sigma^2 d2(p)/dx2 + 2 * sigma d(sigma)/dx * d(p)/dx + ((d(sigma)/dx)^2 + sigma * d2(sigma)/dx2) * p
                    var volUp = _smile.GetVol(fwd, fwd * System.Math.Exp(k + dk));
                    var vol = _smile.GetVol(fwd, strike);
                    var volDown = _smile.GetVol(fwd, fwd * System.Math.Exp(k - dk));
                    var dsdx = 0.5 * (volUp - volDown) / dk;
                    var d2sdx2 = (volUp + volDown - 2.0 * vol) / (dk * dk);

                    var mu = 0;// _curve.GetInstantaneousForwardYield(_expiry) - 0.5 * vol * vol;
                    var dmupdK = -mu * dpdx;
                    var d2 = 0.5 * vol * vol * d2pdx2 + vol * dsdx * dpdx + 0.5 * (dsdx * dsdx + vol * d2sdx2) * pdf;

                    dp[index] = (dmupdK + d2) * _dt;
                });
            }

            Task.WaitAll(tasks);

            var interpolator = new PiecewiseLinearInterpolator<FlatExtrapolator, FlatExtrapolator>(_customDistribution.X, dp);
            var newVolPoints = ComputeVolPoints(fwd, interpolator);

            _expiry += _dt;
            var newFwd = _curve.GetForward(_spot, _expiry);

            _smile = new SABR(_smile.SmileDate, _smile.SmileExpiry.AddDays(_dt * 365), newFwd);
            var success = SmileOptimizer.CalibrateSmile(newFwd, ref _smile, newVolPoints.Select(p => new SmileOptimizer.CallOption(newFwd, p.Strike, p.Volatility, _expiry)).ToArray());

            if (success)
            {
                _customDistribution = new CustomDistribution(newFwd, _smile);
            } else throw new Exception();
        }

        public void ComputeMoments()
        {
            _customDistribution.ComputeMoments();
        }

        private VolPoint[] ComputeVolPoints(double fwd, IInterpolator interpolator)
        {
            var res = new VolPoint[_customDistribution.SubX.Length];
            var tasks = new Task[res.Length];

            for (int i = 0; i < _customDistribution.SubX.Length; ++i)
            {
                var index = i;

                tasks[i] = Task.Run(() =>
                {
                    var k = _customDistribution.SubX[index];
                    var strike = fwd * System.Math.Exp(k);
                    var volatility = _smile.GetVol(fwd, strike);

                    var callUp = SmileOptimizer.CallPrice(fwd, strike, volatility + dVol, _expiry);
                    var call = SmileOptimizer.CallPrice(fwd, strike, volatility, _expiry);
                    var callDown = SmileOptimizer.CallPrice(fwd, strike, volatility - dVol, _expiry);

                    var vega = (callUp - call) * one_over_dVol;
                    var vomma = (callUp + callDown - 2.0 * call) * one_over_dVolSq;

                    var dCall = dCalldt(k, fwd, interpolator);
                    var dVolatility = dCall / vega - 0.5 * vomma / System.Math.Pow(vega, 3) * dCall * dCall;

                    res[index] = new VolPoint(strike, volatility + dVolatility);
                });
            }

            Task.WaitAll(tasks);

            return res;
        }

        private Func<double, double, IInterpolator, double> dCalldt = (k, fwd, interpolator) =>
        {
            var strike = fwd * System.Math.Exp(k);
            return MathNet.Numerics.Integration.GaussKronrodRule.Integrate((x) => (fwd * System.Math.Exp(x) - strike) * interpolator.Interpolate(x), k, 15, out var err, out var norm);
        };
    }
}
