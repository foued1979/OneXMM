using System;

namespace OneXMM.Math.Interpolators.Base
{
    using OneXMM.Interfaces;

    public abstract class RateInterpolator : IRateInterpolator
    {

        public double[] Tenors { get; protected set; } = [];
        public double[] Rates { get; protected set; } = [];

        public abstract void Set(double[] tenors, double[] rates);
        public abstract void Set(double[] tenors, double[] rates, RateInterpolationTypes rateInterpolationType);
        public abstract double Interpolate(double expiry);

        protected int Locate(double tenor)
        {
            if (tenor < Tenors.First() || tenor > Tenors.Last()) return -1;
            
            var index = 0;

            foreach (var t in Tenors)
            {
                if (tenor < t) return index;
                index++;
            }

            return -1;
        }
    }
}
