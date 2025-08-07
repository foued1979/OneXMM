using System;

namespace OneXMM.Math.Extrapolators
{
    using OneXMM.Interfaces;

    public abstract class RateExtrapolator : IRateExtrapolator
    {
        public abstract double Extrapolate(double value);
        public abstract void Set(double[] x, double[] y, RateInterpolationTypes rateInterpolationType, ExtrapolatorTypes extrapolatorType);
    }
}
