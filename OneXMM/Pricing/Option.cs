using System;

namespace OneXMM.Pricing
{
    using OneXMM;
    using OneXMM.Communication;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;

    public sealed class Option : IComparable<Option>, IMySQLData
    {
        public int Id { get; set; }
        public IExchange Exchange { get; set; }
        public string Symbol { get; set; }
        public Pair Pair { get; set; }
        public OptionTypes OptionType { get; set; }
        public double Strike { get; }
        public DateTime Expiry { get; }
        public bool IsDiscounted { get; }
        public bool IsQuotedInBase { get; }

        public Option(IExchange exchange, Pair pair, OptionTypes optionType, double strike, DateTime expiry)
            : this(exchange, CreateSymbol(pair, optionType, expiry, strike, exchange), pair, optionType, strike, expiry)
        { }

        public Option(IExchange exchange, string symbol, Pair pair, OptionTypes optionType, double strike, DateTime expiry)
            : this(symbol.GetHashCode(), exchange, symbol, pair, optionType, strike, expiry) { }

        public Option(int id, IExchange exchange, string symbol, Pair pair, OptionTypes optionType, double strike, string expiry)
            : this(id, exchange, symbol, pair, optionType, strike, exchange.ToExpiry(expiry)) { }

        public Option(int id, IExchange exchange, string symbol, Pair pair, OptionTypes optionType, double strike, DateTime expiry)
        {
            Id = id;
            Exchange = exchange;
            Symbol = symbol;
            Pair = pair;
            OptionType = optionType;
            Strike = strike;
            IsDiscounted = exchange.IsDiscounted;
            IsQuotedInBase = exchange.IsQuotedInBase;

            // The Exchange::ToExpiry includes the UseUniversalTime check
            Expiry = exchange.ToExpiry(expiry);
        }

        public double Price(DateTime asof, double spot, IVolatilitySurface volSurface)
        {
            if (Exchange.ExchangeUsesUniversalTime) asof = asof.ToUniversalTime();

            var expiry = (Expiry - volSurface.SurfaceDate).TotalDays * Constants.OneOver365;
            var forward = volSurface.GetForward(spot, Expiry);
            var vol = volSurface.GetVol(forward, Strike, Expiry);

            var volSqrtT = vol * System.Math.Sqrt(expiry);
            var d1 = System.Math.Log(forward / (double)Strike) / volSqrtT + 0.5 * volSqrtT;
            var d2 = d1 - volSqrtT;
            var nsd = new Accord.Statistics.Distributions.Univariate.NormalDistribution(0, 1);

            var priceAdjust = Exchange.IsQuotedInBase ? 1.0 : 1.0 / spot;
            if (Exchange.IsDiscounted) priceAdjust *= spot / forward;

            return OptionType == OptionTypes.Call
                ? priceAdjust * (forward * nsd.DistributionFunction(d1) - Strike * nsd.DistributionFunction(d2))
                : priceAdjust * (Strike * nsd.DistributionFunction(-d2) - forward * nsd.DistributionFunction(-d1));
        }

        public double Price(DateTime asof, double spot, double forward, ISmile smile)
        {
            if (Exchange.ExchangeUsesUniversalTime) asof = asof.ToUniversalTime();

            var expiry = (Expiry - smile.SmileDate).TotalDays * Constants.OneOver365;
            var vol = smile.GetVol(forward, Strike);

            var volSqrtT = vol * System.Math.Sqrt(expiry);
            var d1 = System.Math.Log(forward / Strike) / volSqrtT + 0.5 * volSqrtT;
            var d2 = d1 - volSqrtT;
            var nsd = new Accord.Statistics.Distributions.Univariate.NormalDistribution(0, 1);

            var priceAdjust = Exchange.IsQuotedInBase ? 1.0 : 1.0 / spot;
            if (Exchange.IsDiscounted) priceAdjust *= spot / forward;

            return OptionType == OptionTypes.Call
                ? priceAdjust * (forward * nsd.DistributionFunction(d1) - Strike * nsd.DistributionFunction(d2))
                : priceAdjust * (Strike * nsd.DistributionFunction(-d2) - forward * nsd.DistributionFunction(-d1));
        }

        public Option Copy()
        {
            return new Option(Id, Exchange, Symbol, Pair, OptionType, Strike, Expiry);
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
