using System;

namespace OneXMM.Referential
{
    using OneXMM.Pricing;
    using OneXMM.Referential.Exchanges;

    public sealed class Option : IComparable<Option>
    {
        public int Id { get; set; }
        public Exchange Exchange { get; set; }
        public string Symbol { get; set; }
        public Assets.Pair Pair { get; set; }
        public OptionTypes OptionType { get; set; }
        public double Strike { get; }
        public DateTime Expiry { get; }

        public Option() { }

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

            Console.WriteLine($"Vol: {vol}");

            var volSqrtT = vol * System.Math.Sqrt(expiry);
            var d1 = System.Math.Log(forward / Strike) / volSqrtT + 0.5 * volSqrtT;
            var d2 = d1 - volSqrtT;
            var nsd = new MathNet.Numerics.Distributions.Normal(0, 1);

            var priceAdjust = Exchange.QuoteInBase ? 1.0 : 1.0 / spot;
            if (Exchange.IsDiscounted) priceAdjust *= spot / forward;

            return OptionType == OptionTypes.Call
                ? priceAdjust * (forward * nsd.CumulativeDistribution(d1) - Strike * nsd.CumulativeDistribution(d2))
                : priceAdjust * (Strike * nsd.CumulativeDistribution(-d2) - forward * nsd.CumulativeDistribution(-d1));
        }

        public double Price(DateTime asof, double spot, double forward, SABR smile)
        {
            var expiry = (Expiry - smile.SmileDate).TotalDays * Constants.OneOver365;
            var vol = smile.GetVol(forward, Strike);

            var volSqrtT = vol * System.Math.Sqrt(expiry);
            var d1 = System.Math.Log(forward / Strike) / volSqrtT + 0.5 * volSqrtT;
            var d2 = d1 - volSqrtT;
            var nsd = new MathNet.Numerics.Distributions.Normal(0, 1);

            var priceAdjust = Exchange.QuoteInBase ? 1.0 : 1.0 / spot;
            if (Exchange.IsDiscounted) priceAdjust *= spot / forward;

            return OptionType == OptionTypes.Call
                ? priceAdjust * (forward * nsd.CumulativeDistribution(d1) - Strike * nsd.CumulativeDistribution(d2))
                : priceAdjust * (Strike * nsd.CumulativeDistribution(-d2) - forward * nsd.CumulativeDistribution(-d1));
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
