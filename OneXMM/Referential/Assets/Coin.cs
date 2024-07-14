using System;

namespace OneXMM.Referential.Assets
{
    public sealed class Coin
    {
        public string Symbol { get; } = "";
        public string Address { get; } = "";

        public Coin(string symbol, string address) 
        { 
            Symbol = symbol;
            Address = address;
        }

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
