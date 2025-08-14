using System;

namespace OneXMM.Math.Extrapolators
{
    using OneXMM.Math.Extrapolators.Base;

    public sealed class LinearRateExtrapolator : RateExtrapolator
    {
        private double _xRef;
        private double _yRef;
        private double _slope;
        private bool _isOnFwd;

        public override double Extrapolate(double value)
        {
            var extrap = _yRef + _slope * (value - _xRef);

            if (_isOnFwd) return (extrap - 1) / value;
            return extrap;
        }

        public override void Set(double[] x, double[] y, RateInterpolationTypes rateInterpolationType, ExtrapolatorTypes extrapolatorType)
        {
            if (x.Length < 2 || y.Length != x.Length) throw new Exception("LinearRateExtrapolator - Data is not consistent.");

            _isOnFwd = (rateInterpolationType & RateInterpolationTypes.Forwards) != 0;

            if (extrapolatorType == ExtrapolatorTypes.Down)
            {
                _yRef = y[0];
                _xRef = x[0];
                _slope = (y[1] - y[0]) / (x[1] - x[0]);
            }
            else
            {
                var lastIndex = y.Length - 1;
                _yRef = y[lastIndex];
                _xRef = x[lastIndex];
                _slope = (y[lastIndex] - y[lastIndex - 1]) / (x[lastIndex] - x[lastIndex - 1]);
            }
        }
    }
}
