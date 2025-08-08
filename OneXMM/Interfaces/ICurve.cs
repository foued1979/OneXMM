using System;

namespace OneXMM.Interfaces
{
    public interface ICurve
    {
        double GetForwardYield(double expiry);
        double GetForward(double spot, double expiry);
        double GetForward(double spot, DateTime expiry);
        double GetInstantaneousForwardYield(double expiry);
    }
}
