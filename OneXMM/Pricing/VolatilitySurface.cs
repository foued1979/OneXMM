using System;

namespace OneXMM.Pricing
{
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    public class VolatilitySurface
    {
        public DateTime SurfaceDate { get; set; }

        public Pair Pair { get; set; }

        public SortedDictionary<DateTime, SABR> Smiles { get; set; } = new SortedDictionary<DateTime, SABR>();

        public SortedDictionary<DateTime, double> Yields { get; set; } = new SortedDictionary<DateTime, double>();

        public VolatilitySurface() { }

        public VolatilitySurface(DateTime surfaceDate, Pair pair)
        {
            Pair = pair;
            SurfaceDate = surfaceDate; 
        }

        public double GetForward(double spot, DateTime asof, DateTime expiry)
        {
            var yf = 0.0;

            if (Yields.First().Key.CompareTo(expiry) > 0)
            {
                yf = (Yields.First().Key - asof).TotalDays / 365.0;
                return spot * (1 + Yields.First().Value * yf);
            }

            var expiries = Yields.Keys;
            yf = (expiry - asof).TotalDays / 365.0;

            for (int i = 0; i < Yields.Count; i++)
            {
                if (expiry.IsAround(expiries.ElementAt(i)))
                {
                    return spot * (1 + Yields.ElementAt(i).Value * yf);
                }
                else if (expiry.CompareTo(expiries.ElementAt(i)) < 0)
                {
                    var tenorDown = (Yields.ElementAt(i - 1).Key - asof).TotalDays / 365.0;
                    var yieldDown = Yields.ElementAt(i - 1).Value;

                    var tenorUp = (Yields.ElementAt(i).Key - asof).TotalDays / 365.0;
                    var yieldUp = Yields.ElementAt(i).Value;

                    var yield = yieldDown + (yf - tenorDown) / (tenorUp - tenorDown) * (yieldUp - yieldDown);
                    return spot * (1 + yield * yf);
                }
            }

            return spot * (1 + Yields.Last().Value * yf);
        }

        public double GetVol(double forward, double strike, DateTime expiry)
        {
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

        public void Serialize()
        {
            var now = DateTime.Now.ToUniversalTime();
            var surfaceDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
            var path = Path.Combine(Environment.CurrentDirectory, $"Configuration\\VolSurface_{surfaceDate.ToString("dd-MM-yyyyTHH_mm_ss")}.json");

            Utils.Serialize(this, path, "dd-MM-yyyyTHH:mm:ss");
        }
    }
}
