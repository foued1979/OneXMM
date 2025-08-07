using System;

namespace OneXMM.Math.Extrapolators
{
    public sealed class FlatRateExtrapolator : RateExtrapolator
    {
        private bool _isOnFwd;
        private double _rate;

        public FlatRateExtrapolator() { }

        public override double Extrapolate(double value)
        {
            return _rate;
        }

        public override void Set(double[] x, double[] y, RateInterpolationTypes rateInterpolationType, ExtrapolatorTypes extrapolatorType)
        {
            _isOnFwd = (rateInterpolationType & RateInterpolationTypes.Forwards) != 0;
            var rate = extrapolatorType == ExtrapolatorTypes.Down ? y.First() : y.Last();

            _rate = _isOnFwd ? (rate - 1) / x.First() : rate;
        }
    }
}
