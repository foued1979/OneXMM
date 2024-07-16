using System;

namespace OneXMM.Referential
{
    using OneXMM.Pricing;
    using OneXMM.Referential.Exchanges;

    public sealed class Option : IComparable<Option>
    {
        public int Id { get; }
        public Exchange Exchange { get; }
        public string Symbol { get; }
        public Assets.Pair Pair { get; }
        public OptionTypes OptionType { get; }

        public Option(OptionTypes optionType)
        {
            OptionType = optionType;
        }

        public double Strike { get; }
        public DateTime Expiry { get; }

        public Option(int id, Exchange exchange, string symbol, Assets.Pair pair, OptionTypes optionType, double strike, string expiry)
        {
            Id = id;
            Exchange = exchange;
            Symbol = symbol;
            Pair = pair;
            OptionType = optionType;
            Strike = strike;
            Expiry = exchange.ToExpiry(expiry);
        }

        public double Price(DateTime asof, double spot, VolatilitySurface volSurface)
        {
            var expiry = (Expiry - volSurface.SurfaceDate).TotalDays * Constants.OneOver365;
            var forward = volSurface.GetForward(spot, asof, Expiry);
            var vol = volSurface.GetVol(forward, Strike, Expiry);

            var volSqrtT = vol * System.Math.Sqrt(expiry);
            var d1 = System.Math.Log(forward / Strike) / volSqrtT + 0.5 * volSqrtT;
            var d2 = d1 - volSqrtT;
            var nsd = new MathNet.Numerics.Distributions.Normal(0, 1);

            return OptionType == OptionTypes.Call
                ? nsd.CumulativeDistribution(d1) - Strike / forward * nsd.CumulativeDistribution(d2)
                : Strike / forward * nsd.CumulativeDistribution(-d2) - nsd.CumulativeDistribution(-d1);
        }

        public double Price(DateTime asof, double forward, SABR smile)
        {
            var expiry = (Expiry - smile.SmileDate).TotalDays * Constants.OneOver365;
            var vol = smile.GetVol(forward, Strike);

            var volSqrtT = vol * System.Math.Sqrt(expiry);
            var d1 = System.Math.Log(forward / Strike) / volSqrtT + 0.5 * volSqrtT;
            var d2 = d1 - volSqrtT;
            var nsd = new MathNet.Numerics.Distributions.Normal(0, 1);

            return OptionType == OptionTypes.Call
                ? nsd.CumulativeDistribution(d1) - Strike / forward * nsd.CumulativeDistribution(d2)
                : Strike / forward * nsd.CumulativeDistribution(-d2) - nsd.CumulativeDistribution(-d1);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public int CompareTo(Option? other)
        {
            if (other is null) return -1;

            var expiryCompare = Expiry.CompareTo(other.Expiry);
            return expiryCompare == 0 ? Strike.CompareTo(other.Strike) : expiryCompare;
        }
    }
}
