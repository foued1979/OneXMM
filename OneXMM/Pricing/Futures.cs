using System;

namespace OneXMM.Pricing
{
    using OneXMM.Communication;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges;

    public sealed class Futures : IMySQLData
    {
        public int Id { get; }
        public double Yield { get; private set; }
        public IExchange Exchange { get; }
        public string Symbol { get; }
        public Pair Pair { get; }
        public DateTime Expiry { get; }

        public Futures(Pair pair, Exchange exchange, DateTime expiry)
        {
            Id = -1;
            Symbol = string.Format("{0}-{1}-{2}", pair.Symbol, exchange.Name, expiry.ToString("ddMMMyy"));
            Exchange = exchange;
            Pair = pair;
            Expiry = expiry;
        }

        public Futures(int id, Exchange exchange, string symbol, Pair pair, DateTime expiry)
        {
            Id = id;
            Exchange = exchange;
            Symbol = symbol;
            Pair = pair;

            Expiry = exchange.ToExpiry(expiry);
        }

        public Futures(int id, Exchange exchange, string symbol, Pair pair, string expiry)
        {
            Id = id;
            Exchange = exchange;
            Symbol = symbol;
            Pair = pair;

            Expiry = exchange.ToExpiry(expiry);
        }

        public void SetYield(DateTime time, double spot, double marketPrice)
        {
            var expiry = (Expiry - time).TotalDays * Constants.OneOver365;
            Yield = (marketPrice / spot - 1.0) / expiry;
        }

        public double GetForwardValue(DateTime asof, double spot)
        {
            var expiry = (Expiry - asof).TotalDays * Constants.OneOver365;
            return spot * (1 + Yield * expiry);
        }

        public string[] ToFields()
        {
            // Table format in MySQLConnector class
            // Current format: symbol, undl, expiry, exchange
            return [Symbol, Pair.Symbol, Expiry.ToString(MySQLConnector.DateFormat), Exchange!.Name];
        }
    }
}
