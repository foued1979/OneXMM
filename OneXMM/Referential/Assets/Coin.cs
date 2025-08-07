using System;

namespace OneXMM.Referential.Assets
{
    using OneXMM.Referential.DataFeeds;

    public sealed class Coin : IComparable<Coin>
    {
        public int Id { get; set; }
        public bool IsFiat => string.IsNullOrEmpty(Address);
        public bool IsNative => Address.Equals("0x");
        public string ContractSymbol { get; set; } = "";
        public string InternalSymbol { get; set; } = "";
        public string Address { get; set; } = "";
        public Blockchains Chain { get; set; } = Blockchains.Error;
        public Dictionary<DataFeeders, DataFeedInfo> DataFeedInfo { get; set; } = new Dictionary<DataFeeders, DataFeedInfo>();

        public Coin() { }

        public Coin(int id, string contractSymbol, string internalSymbol, string address, Blockchains chain, Dictionary<DataFeeders, DataFeedInfo> dataFeedInfo) 
        { 
            Id = id;
            ContractSymbol = contractSymbol;
            InternalSymbol = internalSymbol;
            Address = address;
            Chain = chain;
            DataFeedInfo = dataFeedInfo;
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
