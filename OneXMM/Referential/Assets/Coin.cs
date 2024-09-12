using System;

namespace OneXMM.Referential.Assets
{
    public sealed class Coin
    {
        public bool IsFiat { get; }
        public string Symbol { get; } = "";
        public string Address { get; } = "";
        public Blockchains Chain { get; } = Blockchains.Error;

        public Coin(string symbol, string address, Blockchains chain) 
        { 
            Symbol = symbol;
            Address = address;
            Chain = chain;

            IsFiat = string.IsNullOrEmpty(address);
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
