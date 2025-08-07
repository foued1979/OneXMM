using System;

namespace OneXMM.Interfaces
{
    public interface IInterpolator
    {
        double[] X { get; }
        double[] Y { get; }

        void Set(double[] tenors, double[] rates);
        void Set(double[] tenors, double[] rates, RateInterpolationTypes rateInterpolationType);
        double Interpolate(double expiry);
    }
}
