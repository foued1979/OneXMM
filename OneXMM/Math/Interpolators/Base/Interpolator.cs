using System;

namespace OneXMM.Math.Interpolators.Base
{
    using Accord.Statistics.Kernels;
    using OneXMM.Interfaces;

    public abstract class Interpolator : IInterpolator
    {
        public double[] X { get; protected set; } = [];

        public double[] Y { get; protected set; } = [];

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

        public abstract double Interpolate(double x);

        public abstract void Set(double[] x, double[] y);
    }
}
