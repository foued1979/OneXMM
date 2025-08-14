using System;

namespace OneXMM.Math.Extrapolators
{
    using OneXMM.Math.Extrapolators.Base;

    public sealed class LinearExtrapolator : Extrapolator
    {
        private double _slope;
        private double _xRef;
        private double _yRef;

        public override double Extrapolate(double value)
        {
            return _yRef + _slope * (value - _xRef);
        }

        public override void Set(double[] x, double[] y, ExtrapolatorTypes extrapolatorType)
        {
            if (x.Length < 2 || x.Length != y.Length) throw new Exception("LinearExtrapolator - Data is not consistent.");

            switch (extrapolatorType)
            {
                case ExtrapolatorTypes.Down:
                    _xRef = x[0];
                    _yRef = y[0];
                    _slope = (y[1] - y[0]) / (x[1] - x[0]);
                    break;
                case ExtrapolatorTypes.Up:
                    var lastIndex = x.Length - 1;
                    _xRef = x.Last();
                    _yRef = y.Last();
                    _slope = (y[lastIndex] - y[lastIndex - 1]) / (x[lastIndex] - x[lastIndex - 1]);
                    break;

            }
        }
    }
}
