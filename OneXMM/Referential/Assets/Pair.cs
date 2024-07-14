using System;

namespace OneXMM.Referential.Assets
{
    public sealed class Pair
    {
        public Coin Coin { get; }
        public Coin Base { get; }
        public string Symbol => string.Format("{0}/{1}", Coin.Symbol, Base.Symbol);

        public Pair(Coin quote, Coin counter)
        {
            Coin = quote;
            Base = counter;
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
