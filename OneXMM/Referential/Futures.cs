using System;

namespace OneXMM.Referential
{
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges;

    public sealed class Futures
    {
        public static Futures GetNoExpiryFutures(Pair pair, DateTime asof) => new Futures(pair, asof);
        public int Id { get; }
        public double Yield { get; private set; }
        public Exchange? Exchange { get; }
        public string Symbol { get; }
        public Pair Pair { get; }
        public DateTime Expiry { get; }

        public Futures() { }

        private Futures(Pair pair, DateTime expiry)
        {
            Id = -1;
            Symbol = "NoExp";
            Pair = pair;
            Expiry = expiry;
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
            var expiry = (Expiry - time).TotalDays / 365.0;
            Yield = (marketPrice / spot - 1.0) / expiry;
        }
    }
}
