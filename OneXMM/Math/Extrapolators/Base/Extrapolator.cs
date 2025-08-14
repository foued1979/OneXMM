using System;

namespace OneXMM.Math.Extrapolators.Base
{
    using OneXMM.Interfaces;

    public abstract class Extrapolator : IExtrapolator
    {
        public abstract double Extrapolate(double value);

        public abstract void Set(double[] x, double[] y, ExtrapolatorTypes extrapolatorType);
    }
}
