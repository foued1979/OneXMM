using System;

namespace OneXMM.Interfaces
{
    public interface IExtrapolator
    {
        void Set(double[] x, double[] y, ExtrapolatorTypes extrapolatorType);
        double Extrapolate(double value);
    }
}
