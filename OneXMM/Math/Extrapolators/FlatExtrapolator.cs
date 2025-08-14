using OneXMM.Math.Extrapolators.Base;
using System;

namespace OneXMM.Math.Extrapolators
{
    public sealed class FlatExtrapolator : Extrapolator
    {
        private double _lastY;

        public override double Extrapolate(double value)
        {
            return _lastY;
        }

        public override void Set(double[] x, double[] y, ExtrapolatorTypes extrapolatorType)
        {
            switch (extrapolatorType)
            {
                case ExtrapolatorTypes.Up:
                    _lastY = y.Last();
                    break;
                case ExtrapolatorTypes.Down:
                    _lastY = y.First();
                    break;
            }
        }
    }
}
