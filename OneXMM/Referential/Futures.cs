using System;

namespace OneXMM.Referential
{
    using OneXMM.Referential.Exchanges;

    public sealed class Futures
    {
        public double Yield { get; private set; }
        public Exchange Exchange { get; }
        public string Symbol { get; }
        public Assets.Pair Pair { get; }
        public DateTime Expiry { get; }

        public Futures(Exchange exchange, string symbol, Assets.Pair pair, string expiry)
        {
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
