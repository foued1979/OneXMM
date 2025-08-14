using System;

namespace OneXMM.Math.Interpolators
{
    using OneXMM.Interfaces;
    using OneXMM.Math.Interpolators.Base;
    using OneXMM.Referential;

    public sealed class PiecewiseLinearRateInterpolator<TExtrapolatorDown, TExtrapolatorUp> : RateInterpolator
        where TExtrapolatorDown : IRateExtrapolator, new()
        where TExtrapolatorUp : IRateExtrapolator, new()
    {
        private readonly TExtrapolatorDown _extrapolatorDown = new();
        private readonly TExtrapolatorUp _extrapolatorUp = new();
        private double[] _slopes = [];
        private bool _isOnFwd;

        public RateInterpolationTypes RateInterpolationType { get; private set; }

        public PiecewiseLinearRateInterpolator() { }

        public PiecewiseLinearRateInterpolator(RateInterpolationTypes rateInterpolationType) 
        {
            RateInterpolationType = rateInterpolationType;
            _isOnFwd = (RateInterpolationType & RateInterpolationTypes.Forwards) != 0;
        }

        public PiecewiseLinearRateInterpolator(DateTime asof, Futures[] futures, RateInterpolationTypes rateInterpolationType) : this(rateInterpolationType)
        {
            if (futures.Length < 2) throw new ArgumentException("PiecewiseLinearRateInterpolator - The interpolator requires at least 2 futures.");

            var rates = new List<double>(futures.Length);
            var tenors = new List<double>(futures.Length);

            foreach (var future in futures)
            {
                tenors.Add((future.Expiry - asof).TotalDays * Constants.OneOver365);
                rates.Add(future.Yield);
            }

            Set(tenors.ToArray(), rates.ToArray());
            _extrapolatorDown.Set(Tenors, Rates, rateInterpolationType, ExtrapolatorTypes.Down);
            _extrapolatorUp.Set(Tenors, Rates, rateInterpolationType, ExtrapolatorTypes.Up);
        }

        public PiecewiseLinearRateInterpolator(double[] tenors, double[] rates, RateInterpolationTypes rateInterpolationType) : this(rateInterpolationType)
        {
            if (rates.Length != tenors.Length) throw new ArgumentException("PiecewiseLinearRateInterpolator - Number of tenors and rates are not the same.");
            if (rates.Length < 2) throw new ArgumentException("PiecewiseLinearRateInterpolator - The interpolator requires at least 2 rates.");

            Set(tenors, rates);
            _extrapolatorDown.Set(Tenors, Rates, rateInterpolationType, ExtrapolatorTypes.Down);
            _extrapolatorUp.Set(Tenors, Rates, rateInterpolationType, ExtrapolatorTypes.Up);
        }

        public override void Set(double[] tenors, double[] rates, RateInterpolationTypes rateInterpolationType)
        {
            RateInterpolationType = rateInterpolationType;
            _isOnFwd = (RateInterpolationType & RateInterpolationTypes.Forwards) != 0;

            Set(tenors, rates);
            _extrapolatorDown.Set(Tenors, Rates, rateInterpolationType, ExtrapolatorTypes.Down);
            _extrapolatorUp.Set(Tenors, Rates, rateInterpolationType, ExtrapolatorTypes.Up);
        }

        public unsafe override void Set(double[] tenors, double[] rates)
        {
            Tenors = tenors;
            Rates = rates;

            _slopes = new double[tenors.Length - 1];
            var isOnFwd = _isOnFwd;

            fixed (double* p_slope0 = &_slopes[0])
            fixed (double* p_tenor0 = &Tenors[0])
            fixed (double* p_rate0 = &Rates[0])
            {
                double* p_slope = p_slope0;
                double* p_rate = p_rate0;
                double* p_tenor = p_tenor0;

                for (int i = 0; i < _slopes.Length; ++i)
                {
                    double* next_p_rate = p_rate + 1;
                    double* next_p_tenor = p_tenor + 1;

                    if (isOnFwd)
                    {
                        *p_rate = 1 + *p_rate * *p_tenor;
                        *next_p_rate = 1 + *next_p_rate * *next_p_tenor;
                    }

                    *p_slope = (*next_p_rate - *p_rate) / (*next_p_tenor - *p_tenor);

                    p_slope++;
                    p_rate++;
                    p_tenor++;
                }
            }
        }

        public override double Interpolate(double expiry)
        {
            if (expiry <= Tenors.First()) return _extrapolatorDown.Extrapolate(expiry);
            if (expiry >= Tenors.Last()) return _extrapolatorUp.Extrapolate(expiry);

            int index = Locate(expiry);

            var interp = Rates[index - 1] + _slopes[index - 1] * (expiry -  Tenors[index - 1]);

            if (_isOnFwd) return (interp - 1) / expiry;
            return interp;
        }
    }
}
