using System;

namespace OneXMM.Interfaces
{
    public interface IRateExtrapolator
    {
        void Set(double[] tenors, double[] rates, RateInterpolationTypes rateInterpolationType, ExtrapolatorTypes extrapolatorType);
        double Extrapolate(double value);
    }
}
