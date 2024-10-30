using System;

namespace OneXMM.Referential.Assets
{
    public sealed class Coin : IComparable<Coin>
    {
        public int Id { get; }
        public bool IsFiat => string.IsNullOrEmpty(Address);
        public bool IsNative => Address.Equals("0x");
        public string ContractSymbol { get; } = "";
        public string InternalSymbol { get; } = "";
        public string Address { get; } = "";
        public Blockchains Chain { get; } = Blockchains.Error;

        public Coin(int id, string contractSymbol, string internalSymbol, string address, Blockchains chain) 
        { 
            Id = id;
            ContractSymbol = contractSymbol;
            InternalSymbol = internalSymbol;
            Address = address;
            Chain = chain;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Coin c)
            {
                return Id == c.Id;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return InternalSymbol.GetHashCode();
        }

        public int CompareTo(Coin? other)
        {
            if (other is null) return -1;
            if (other.Equals(this)) return 0;

            if (other.Chain != Chain) return Chain.CompareTo(other.Chain);
            if (other.Address.Equals(Address)) return InternalSymbol.CompareTo(other.InternalSymbol);

            return Address.CompareTo(other.Address);
        }
    }
}
