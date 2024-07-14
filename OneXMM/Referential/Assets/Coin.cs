using System;

namespace OneXMM.Referential.Assets
{
    public sealed class Coin
    {
        public string Symbol { get; } = "";

        public Coin(string symbol) { Symbol = symbol; }

        public override bool Equals(object? obj)
        {
            if (obj is Coin c) return c.Symbol.Equals(Symbol);
            return false;
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode();
        }
    }
}
