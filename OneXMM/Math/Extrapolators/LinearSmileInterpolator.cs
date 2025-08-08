using System;

namespace OneXMM.Math.Extrapolators
{
    using OneXMM.Interfaces;

    public sealed class LinearSmileInterpolator
    {
        private readonly ISmile _prevSmile;
        private readonly ISmile _nextSmile;
        private readonly ICurve _forwardCurve;

        public LinearSmileInterpolator(ISmile[] smiles, ICurve forwardCurve) 
        {
            _prevSmile = smiles[smiles.Length - 2];
            _nextSmile = smiles.Last();
            _forwardCurve = forwardCurve;
        }

        public double Extrapolate(double spot, double strike, DateTime expiry)
        {
            var exp = (expiry - _nextSmile.SmileDate).TotalDays * Constants.OneOver365;
            var prevVol = _prevSmile.GetVol(_forwardCurve.GetForward(spot, _prevSmile.SmileExpiry), strike);
            var nextVol = _nextSmile.GetVol(_forwardCurve.GetForward(spot, _nextSmile.SmileExpiry), strike);

            var prevVar = prevVol * prevVol * _prevSmile.Expiry;
            var nextVar = nextVol * nextVol * _nextSmile.Expiry;

            var slope = (nextVar - prevVar) / (_nextSmile.Expiry - _prevSmile.Expiry);
            var extrapolatedVar = nextVar + slope * (exp - _nextSmile.Expiry);

            return System.Math.Sqrt(extrapolatedVar / exp);
        }
    }
}
