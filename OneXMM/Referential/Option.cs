using System;

namespace OneXMM.Referential
{
    using OneXMM.Communication;
    using OneXMM.Interfaces;
    using OneXMM.Pricing;
    using OneXMM.Referential.Assets;

    public sealed class Option : IComparable<Option>, IMySQLData
    {
        public int Id { get; set; }
        public IExchange Exchange { get; set; }
        public string Symbol { get; set; }
        public Assets.Pair Pair { get; set; }
        public OptionTypes OptionType { get; set; }
        public double Strike { get; }
        public DateTime Expiry { get; }

        public Option(IExchange exchange, Assets.Pair pair, OptionTypes optionType, double strike, DateTime expiry)
            : this(exchange, CreateSymbol(pair, optionType, expiry, strike, exchange), pair, optionType, strike, expiry)
        { }

        public Option(IExchange exchange, string symbol, Assets.Pair pair, OptionTypes optionType, double strike, DateTime expiry)
            : this(symbol.GetHashCode(), exchange, symbol, pair, optionType, strike, expiry) { }

        public Option(int id, IExchange exchange, string symbol, Assets.Pair pair, OptionTypes optionType, double strike, DateTime expiry)
        {
            Id = id;
            Exchange = exchange;
            Symbol = symbol;
            Pair = pair;
            OptionType = optionType;
            Strike = strike;

            // The Exchange::ToExpiry includes the UseUniversalTime check
            Expiry = exchange.ToExpiry(expiry);
        }

        public Option(int id, IExchange exchange, string symbol, Assets.Pair pair, OptionTypes optionType, double strike, string expiry)
        {
            Id = id;
            Exchange = exchange;
            Symbol = symbol;
            Pair = pair;
            OptionType = optionType;
            Strike = strike;

            // The Exchange::ToExpiry includes the UseUniversalTime check
            Expiry = exchange.ToExpiry(expiry);
        }

        public double Price(DateTime asof, double spot, IVolatilitySurface volSurface)
        {
            if (Exchange.ExchangeUsesUniversalTime) asof = asof.ToUniversalTime();

            var expiry = (Expiry - volSurface.SurfaceDate).TotalDays * Constants.OneOver365;
            var forward = volSurface.GetForward(spot, asof, Expiry);
            var vol = volSurface.GetVol(forward, Strike, Expiry);

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
            if (Exchange.ExchangeUsesUniversalTime) asof = asof.ToUniversalTime();

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

        public override bool Equals(object? obj)
        {
            if (obj is Option o)
            {
                return o.Exchange.Name.Equals(Exchange.Name)
                    && o.Pair.Equals(Pair)
                    && o.Expiry.CompareTo(Expiry) == 0
                    && o.Strike == Strike
                    && o.OptionType == OptionType;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public int CompareTo(Option? other)
        {
            if (other is null) return -1;

            if (other.OptionType != OptionType) return OptionType == OptionTypes.Call ? -1 : 1;
            var expiryCompare = Expiry.CompareTo(other.Expiry);
            return expiryCompare == 0 ? Strike.CompareTo(other.Strike) : expiryCompare;
        }

        public string[] ToFields()
        {
            return [Symbol, OptionType.ToString(), Strike.ToString(), Pair.Symbol, Expiry.ToString(MySQLConnector.DateFormat), Exchange.Name];
        }

        private static string CreateSymbol(Pair pair, OptionTypes optionType, DateTime expiry, double strike, IExchange exchange) => $"{pair.Symbol}_{optionType}_{expiry.ToString("ddMMMyy")}_{strike}_{exchange.Name}";
    }
}
