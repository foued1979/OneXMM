using System;

namespace OneXMM.Math.Extrapolators
{
    using OneXMM.Math.Extrapolators.Base;

    public sealed class ZeroExtrapolator : Extrapolator
    {
        public override double Extrapolate(double value)
        {
            return 0;
        }

        public override void Set(double[] x, double[] y, ExtrapolatorTypes extrapolatorType) { }
    }
}
