using System;

namespace OneXMM.Pricing
{
    using OneXMM.Communication;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    public class VolatilitySurface : YieldPoints, IVolatilitySurface
    {
        internal const string DateFormat = "dd-MM-yyyy HH:mm:ss";

        public DateTime SurfaceDate { get; set; }

        public IExchange Exchange { get; set; }

        public Pair Pair { get; set; }

        public SortedDictionary<DateTime, SABR> Smiles { get; set; } = new SortedDictionary<DateTime, SABR>();

        public VolatilitySurface() { }

        public VolatilitySurface(DateTime surfaceDate, Pair pair, IExchange exchange)
        {
            Pair = pair;
            Exchange = exchange;
            SurfaceDate = exchange.ExchangeUsesUniversalTime ? surfaceDate.ToUniversalTime() : surfaceDate; 
        }

        public override double GetForward(double spot, DateTime asof, DateTime expiry)
        {
            if (Exchange.ExchangeUsesUniversalTime)
            {
                asof = asof.ToUniversalTime();
                expiry = expiry.ToUniversalTime();
            }

            return base.GetForward(spot, asof, expiry);
        }

        public double GetVol(double forward, double strike, DateTime expiry)
        {
            if (Exchange.ExchangeUsesUniversalTime) expiry = expiry.ToUniversalTime();

            if (Smiles.Keys.First().CompareTo(expiry) > 0)
            {
                return Smiles.First().Value.GetVol(forward, strike);
            }

            var expiries = Smiles.Keys;

            for (int i = 0; i < Smiles.Count; ++i)
            {
                if (expiry.IsAround(expiries.ElementAt(i)))
                {
                    return Smiles.ElementAt(i).Value.GetVol(forward, strike);
                }
                else if (expiry.CompareTo(expiries.ElementAt(i)) < 0)
                {
                    var one_over_365 = 1.0 / 365.0;
                    var smileDown = Smiles.ElementAt(i - 1).Value;
                    var smileUp = Smiles.ElementAt(i).Value;

                    var volDn = smileDown.GetVol(forward, strike);
                    var volUp = smileUp.GetVol(forward, strike);

                    var yf = (expiry - SurfaceDate).TotalDays * one_over_365;
                    var r = (expiry - smileDown.SmileExpiry).TotalDays * one_over_365;
                    var l = (smileUp.SmileExpiry - smileDown.SmileExpiry).TotalDays * one_over_365;
                    var varianceDn = volDn * volDn * (smileDown.SmileExpiry - SurfaceDate).TotalDays * one_over_365;
                    var varianceUp = volUp * volUp * (smileUp.SmileExpiry - SurfaceDate).TotalDays * one_over_365;

                    return System.Math.Sqrt((varianceDn + r / l * (varianceUp - varianceDn)) / yf);
                }
            }

            return Smiles.Last().Value.GetVol(forward, strike);
        }

        public string[] ToFields()
        {
            return [Pair.Symbol, DateTime.Now.ToUniversalTime().ToString(MySQLConnector.DatetimeFormat), Utils.SerializeToString(this, DateFormat)];
        }
    }
}
