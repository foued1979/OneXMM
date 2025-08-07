using System;

namespace OneXMM.Interfaces
{
    public interface IRateExtrapolator
    {
        void Set(double[] x, double[] y, RateInterpolationTypes rateInterpolationType, ExtrapolatorTypes extrapolatorType);
        double Extrapolate(double value);
    }
}
