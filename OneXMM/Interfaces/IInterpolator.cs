using System;

namespace OneXMM.Interfaces
{
    internal interface IInterpolator
    {
        double[] X { get; }
        double[] Y { get; }
        void Set(double[] x, double[] y);
        double Interpolate(double x);
    }
}
