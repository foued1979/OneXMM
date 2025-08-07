using System;

namespace OneXMM.Pricing.Smiles
{
    using OneXMM.Interfaces;

    public sealed class DupireSmile
    {
        private readonly ISmile _smile;

        public DupireSmile(ISmile smile, ICurve curve)
        {
            _smile = smile;
        }

        public double GetLocalVol(DateTime asof, double forward, double strike)
        {
            var vol = _smile.GetVol(asof, forward, strike);
            var expiry = (_smile.SmileExpiry - asof).TotalDays * Constants.OneOver365;
            var mu = _smile.ForwardYield.HasValue ? _smile.ForwardYield.Value : 0.0;

            var dvar = vol * vol * 1E-6;
            var dK = strike * 1E-8;
            var dVoldK = 0.5 * (_smile.GetVol(asof, forward, strike + dK) - _smile.GetVol(asof, forward, strike - dK)) / dK;
            var d2VoldK2 = (_smile.GetVol(asof, forward, strike + dK) + _smile.GetVol(asof, forward, strike - dK) - 2 * vol) / (dK * dK);

            var num = dvar + 2 * strike * mu * dVoldK * expiry;
            var denom = System.Math.Pow(1 - strike / vol * dVoldK, 2) + vol * expiry * strike * (strike * d2VoldK2 - dVoldK * dVoldK);

            return num / denom;
        }
    }
}
