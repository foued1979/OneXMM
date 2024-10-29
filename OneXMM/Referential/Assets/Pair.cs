using System;

namespace OneXMM.Referential.Assets
{
    using OneXMM;

    public sealed class Pair : IComparable<Pair>
    {
        public string Symbol => $"{Coin.Symbol}/{Base.Symbol}";
        public Blockchains Chain => Coin.Chain;
        public Coin Coin { get; }
        public Coin Base { get; }
        public Dictionary<SymbolTypes, Dictionary<Exchanges, string>> MappingExchangeSymbols { get; }

        public Pair(Coin coin, Coin counter, Dictionary<SymbolTypes, Dictionary<Exchanges, string>> mappingExchangeSymbols)
        {
            Coin = coin;
            Base = counter;
            MappingExchangeSymbols = mappingExchangeSymbols;
        }

        public string GetSymbol(SymbolTypes symbolType, Exchanges exchange)
        {
            if (MappingExchangeSymbols.TryGetValue(symbolType, out var symbByExchange) && symbByExchange.TryGetValue(exchange, out var symbol)) return symbol;
            return string.Empty;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Pair p)
            {
                return p.Coin.Equals(Coin) && p.Base.Equals(Base) && p.Chain == Chain;
            }

            return false;
        }

        public override int GetHashCode() => Symbol.GetHashCode();

        public int CompareTo(Pair? other)
        {
            if (other is null) return -1;
            if (other.Chain != Chain) return Chain.CompareTo(other.Chain);

            if (other.Coin.Equals(Coin)) return Base.CompareTo(other.Base);

            return Coin.CompareTo(other.Coin);
        }
    }
}
