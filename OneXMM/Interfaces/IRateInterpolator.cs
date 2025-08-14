using System;

namespace OneXMM.Interfaces
{
    public interface IRateInterpolator
    {
        double[] Tenors { get; }
        double[] Rates { get; }
        void Set(double[] tenors, double[] rates);
        void Set(double[] tenors, double[] rates, RateInterpolationTypes rateInterpolationType);
        double Interpolate(double expiry);
    }
}
