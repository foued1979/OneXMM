using System;

namespace OneXMM.Interfaces
{
    using OneXMM.Referential.Assets;

    public interface IVolatilitySurface
    {
        DateTime SurfaceDate { get; }
        Pair Pair { get; }

        double GetForward(double spot, DateTime asof, DateTime expiry);

        double GetVol(double forward, double strike, DateTime expiry);
    }
}
