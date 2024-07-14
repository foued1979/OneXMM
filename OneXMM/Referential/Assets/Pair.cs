using System;

namespace OneXMM.Referential.Assets
{
    public sealed class Pair
    {
        public string Symbol => $"{Coin.Symbol}/{Base.Symbol}";
        public Coin Coin { get; }
        public Coin Base { get; }
        public Dictionary<OneXMM.Exchanges, string> ExchangeSymbols { get; }

        public Pair(Coin coin, Coin cBase, Dictionary<OneXMM.Exchanges, string> exchangeSymbols)
        {
            Coin = coin;
            Base = cBase;
            ExchangeSymbols = exchangeSymbols;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Pair p)
            {
                return p.Coin.Equals(Coin) && p.Base.Equals(Base);
            }

            return false;
        }

        public override int GetHashCode() => Symbol.GetHashCode();
    }
}
