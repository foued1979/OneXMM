using System;

namespace OneXMM.Pricing
{
    using OneXMM.Communication;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    public sealed class CorrelatedVolSurface : YieldPoints, IVolatilitySurface
    {
        internal const string DateFormat = "dd-MM-yyyy HH:mm:ss";

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

        public string[] ToFields()
        {
            return [Pair.Symbol, DateTime.Now.ToUniversalTime().ToString(MySQLConnector.DatetimeFormat), Utils.SerializeToString(this, DateFormat)];
        }
    }
}
