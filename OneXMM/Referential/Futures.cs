using System;

namespace OneXMM.Referential
{
    using OneXMM.Referential.Exchanges;

    public sealed class Futures
    {
        public int Id { get; }
        public double Yield { get; private set; }
        public Exchange Exchange { get; }
        public string Symbol { get; }
        public Assets.Pair Pair { get; }
        public DateTime Expiry { get; }

        public Futures(int id, Exchange exchange, string symbol, Assets.Pair pair, string expiry)
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
