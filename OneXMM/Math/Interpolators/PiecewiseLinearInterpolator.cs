using System;

namespace OneXMM.Math.Interpolators
{
    using OneXMM.Interfaces;
    using OneXMM.Math.Interpolators.Base;

    public sealed class PiecewiseLinearInterpolator<TExtrapolatorDown, TExtrapolatorUp> : Interpolator
        where TExtrapolatorDown : IExtrapolator, new()
        where TExtrapolatorUp : IExtrapolator, new()
    {
        private readonly TExtrapolatorDown _extrapolatorDown = new();
        private readonly TExtrapolatorUp _extrapolatorUp = new();
        private double[] _slopes = [];

        public PiecewiseLinearInterpolator() { }

        public PiecewiseLinearInterpolator(double[] x, double[] y)
        {
            Set(x, y);
        }

        public override double Interpolate(double x)
        {
            if (x <= X.First()) return _extrapolatorDown.Extrapolate(x);
            if (x >= X.Last()) return _extrapolatorUp.Extrapolate(x);

            int index = Locate(x);

            return Y[index - 1] + _slopes[index - 1] * (x - X[index - 1]);
        }

        public unsafe override void Set(double[] x, double[] y)
        {
            X = x;
            Y = y;
            _slopes = new double[y.Length - 1];

            fixed (double* p_slope0 = &_slopes[0])
            fixed (double* p_x0 = &X[0])
            fixed (double* p_y0 = &Y[0])
            {
                double* p_slope = p_slope0;
                double* p_y = p_y0;
                double* p_x = p_x0;

                for (int i = 0; i < _slopes.Length; ++i)
                {
                    double* next_p_rate = p_y + 1;
                    double* next_p_tenor = p_x + 1;

                    *p_slope = (*next_p_rate - *p_y) / (*next_p_tenor - *p_x);

                    p_slope++;
                    p_y++;
                    p_x++;
                }
            }
        }
    }
}
