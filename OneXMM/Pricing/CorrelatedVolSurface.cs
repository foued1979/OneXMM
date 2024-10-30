using System;

namespace OneXMM.Pricing
{
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    public sealed class CorrelatedVolSurface : YieldPoints, IVolatilitySurface
    {
        public DateTime SurfaceDate { get; set; }

        public Pair Pair { get; set; }

        public SortedDictionary<DateTime, double> Volatilities { get; set; } = new SortedDictionary<DateTime, double>();

        public CorrelatedVolSurface() { }

        public CorrelatedVolSurface(DateTime surfaceDate, Pair pair)
        {
            SurfaceDate = surfaceDate;
            Pair = pair;
        }

        public override double GetForward(double spot, DateTime asof, DateTime expiry)
        {
            if (SurfaceDate.Kind == DateTimeKind.Utc)
            {
                asof = asof.ToUniversalTime();
                expiry = expiry.ToUniversalTime();
            }

            return base.GetForward(spot, asof, expiry);
        }

        public double GetVol(double forward, double strike, DateTime expiry)
        {
            throw new NotImplementedException();
        }

        public void Serialize()
        {
            var now = DateTime.Now.ToUniversalTime();
            var path = Path.Combine(Environment.CurrentDirectory, $"Configuration\\VolSurface_{SurfaceDate.ToUniversalTime().ToString("dd-MM-yyyyTHH_mm_ss")}.json");

            Utils.Serialize(this, path, "dd-MM-yyyyTHH:mm:ss");
        }
    }
}
