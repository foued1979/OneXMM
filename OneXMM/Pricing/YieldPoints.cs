using System;

namespace OneXMM.Pricing
{
    using OneXMM.Utils;

    public abstract class YieldPoints
    {
        public SortedDictionary<DateTime, double> Yields { get; set; } = new SortedDictionary<DateTime, double>();

        public virtual double GetForward(double spot, DateTime asof, DateTime expiry)
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
    }
}
