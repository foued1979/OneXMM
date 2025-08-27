using System;

namespace OneXMM.Communication.MySQLResults
{
    using OneXMM.Pricing;
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges;

    public struct FuturesResult
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string UndlSymbol { get; set; }
        public DateTime Expiry { get; set; }
        public Exchange Exchange { get; set; }

        public Futures ToFutures(Pair pair)
        {
            // Sanity check
            if (!UndlSymbol.Equals(pair.Symbol)) throw new Exception($"FuturesResult - The Futures definition does not match the pair {pair.Symbol}.");
            return new Futures(Id, Exchange, Symbol, pair, Expiry);
        }

        public static explicit operator Futures[](FuturesResult result)
        {
            var pairs = Config.GetPairs(result.UndlSymbol);
            var futures = new Futures[pairs.Length];

            for (int i = 0; i < futures.Length; ++i)
            {
                futures[i] = new Futures(result.Id, result.Exchange, result.Symbol, pairs[i], result.Expiry);
            }

            return futures;
        }
    }
}
