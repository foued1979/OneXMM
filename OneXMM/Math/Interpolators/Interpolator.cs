using System;

namespace OneXMM.Math.Interpolators
{
    using OneXMM.Interfaces;

    public abstract class Interpolator : IInterpolator
    {

        public double[] X { get; protected set; } = [];
        public double[] Y { get; protected set; } = [];

        public abstract void Set(double[] tenors, double[] rates);
        public abstract void Set(double[] tenors, double[] rates, RateInterpolationTypes rateInterpolationType);
        public abstract double Interpolate(double expiry);

        protected int Locate(double tenor)
        {
            if (tenor < X.First() || tenor > X.Last()) return -1;
            
            var index = 0;

            foreach (var t in X)
            {
                if (tenor < t) return index;
                index++;
            }

            return -1;
        }
    }
}
