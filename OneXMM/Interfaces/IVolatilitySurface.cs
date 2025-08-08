using System;

namespace OneXMM.Interfaces
{
    using OneXMM.Referential.Assets;

    public interface IVolatilitySurface : IMySQLData
    {
        DateTime SurfaceDate { get; }
        Pair Pair { get; }

        double GetForward(double spot, DateTime expiry);

        double GetVol(double spot, double strike, DateTime expiry);
    }
}
