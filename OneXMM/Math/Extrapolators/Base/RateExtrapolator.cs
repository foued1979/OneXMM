using System;

namespace OneXMM.Math.Extrapolators.Base
{
    using OneXMM.Interfaces;

    public abstract class RateExtrapolator : IRateExtrapolator
    {
        public abstract double Extrapolate(double value);
        public abstract void Set(double[] tenors, double[] rates, RateInterpolationTypes rateInterpolationType, ExtrapolatorTypes extrapolatorType);
    }
}
