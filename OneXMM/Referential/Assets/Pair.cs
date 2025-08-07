using System;

namespace OneXMM.Referential.Assets
{
    using OneXMM;

    public sealed class Pair : IComparable<Pair>
    {
        public int Id { get; }
        public const char SymbolSeparator = '/';
        public string Symbol => $"{Coin.InternalSymbol}{SymbolSeparator}{Base.InternalSymbol}";
        public Blockchains Chain => Coin.Chain;
        public Coin Coin { get; }
        public Coin Base { get; }
        public bool CorrelatedVol { get; }
        public Dictionary<SymbolTypes, Dictionary<Exchanges, string>> MappingExchangeSymbols { get; }

        public Pair(int id, Coin coin, Coin counter, bool correlatedVol, Dictionary<SymbolTypes, Dictionary<Exchanges, string>> mappingExchangeSymbols)
        {
            Id = id;
            Coin = coin;
            Base = counter;
            CorrelatedVol = correlatedVol;
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
