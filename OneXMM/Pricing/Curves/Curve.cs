using System;

namespace OneXMM.Pricing.Curves
{
    using OneXMM.Interfaces;
    using OneXMM.Referential;

    public sealed class Curve<TInterpolator> : ICurve
        where TInterpolator : IRateInterpolator, new()
    {
        public DateTime CurveDate { get; }

        private TInterpolator _interpolator = new();

        private Curve(DateTime asof)
        {
            CurveDate = asof;
        }

        public Curve(DateTime asof, double[] tenors, double[] rates, RateInterpolationTypes rateInterpolationType) : this(asof)
        {
            _interpolator.Set(tenors, rates, rateInterpolationType);
        }

        public Curve(DateTime asof, Futures[] futures, RateInterpolationTypes rateInterpolationType) : this(asof)
        {
            var tenors = new double[futures.Length];
            var rates = new double[futures.Length];
            var index = 0;

            foreach (var f in futures)
            {
                tenors[index] = (f.Expiry - asof).TotalDays * Constants.OneOver365;
                rates[index] = f.Yield;
                index++;
            }

            _interpolator.Set(tenors, rates, rateInterpolationType);
        }

        public double GetForwardYield(double expiry)
        {
            return _interpolator.Interpolate(expiry);
        }

        public double GetForward(double spot, double expiry)
        {
            return spot * (1 + _interpolator.Interpolate(expiry) * expiry);
        }

        public double GetInstantaneousForwardYield(double expiry)
        {
            var expiry_dt = expiry + 1E-6;
            return ((1 + _interpolator.Interpolate(expiry_dt) * expiry_dt) / (1 + _interpolator.Interpolate(expiry) * expiry) - 1) * 1E6;
        }

        public double GetForward(double spot, DateTime expiry)
        {
            return GetForward(spot, (expiry - CurveDate).TotalDays * Constants.OneOver365);
        }
    }
}
