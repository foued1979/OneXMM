using System;

namespace OneXMM.Math.Models
{
    using OneXMM.Interfaces;
    using OneXMM.Math.Distributions;

    public sealed class FokkerPlanck
    {
        private readonly double _spot;
        private ISmile _smile;
        private readonly ICurve _curve;
        private readonly CustomDistribution _customDistribution;

        private double _expiry;
        private readonly double _dt;

        public FokkerPlanck(double spot, ISmile smile, ICurve fwdCurve, double dt) 
        {
            _spot = spot;
            _smile = smile;
            _curve = fwdCurve;

            _expiry = smile.Expiry;
            _dt = dt;
            _customDistribution = new CustomDistribution(spot * fwdCurve.GetForward(spot, _expiry), smile);
        }

        // We solve the Fokker-Planck equation for 1 time interval:
        // dp/dt = -d(mu * p)/dx + 0.5 * d2(sigma^2 * p)/dx2
        public void SolveOneStep()
        {
            var tasks = new Task[_customDistribution.X.Length];
            var fwd = _spot * _curve.GetForward(_spot, _expiry);
            var mu = _curve.GetInstantaneousForwardYield(_expiry);

            for (int i = 0; i < _customDistribution.X.Length; i++)
            {
                var index = i;
                var d = _customDistribution.X[i];
                var pdf = _customDistribution.PDF[i];

                tasks[i] = Task.Run(() =>
                {
                    var strike = _smile.GetStrikeFromMoneyness(fwd, d);
                    var dK = strike * 1E-3;

                    var pDown = _customDistribution.Pdf(_smile.GetMoneyness(fwd, strike - dK));
                    var pUp = _customDistribution.Pdf(_smile.GetMoneyness(fwd, strike + dK));
                    var dpdx = 0.5 * (pUp - pDown) / dK;
                    var d2pdx2 = (pUp + pDown - 2.0 * pdf) / (dK * dK);

                    // 0.5 * d2(sigma^2 p)/dx2 = 0.5 * sigma^2 d2(p)/dx2 + 2 * sigma d(sigma)/dx * d(p)/dx + ((d(sigma)/dx)^2 + sigma * d2(sigma)/dx2) * p
                    var volUp = _smile.GetVol(fwd, strike + dK);
                    var vol = _smile.GetVol(fwd, strike);
                    var volDown = _smile.GetVol(fwd, strike - dK);
                    var dsdx = 0.5 * (volUp - volDown) / dK;
                    var d2sdx2 = (volUp + volDown - 2.0 * vol) / (dK * dK);

                    var dmupdK = -mu * dpdx;
                    var d2 = 0.5 * vol * vol * d2pdx2 + 2 * vol * dsdx * dpdx + (dsdx * dsdx + vol * d2sdx2) * pdf;

                    _customDistribution.PDF[index] += (dmupdK + d2) * _dt;
                });
            }

            Task.WaitAll(tasks);
            _expiry += _dt;
        }
    }
}
