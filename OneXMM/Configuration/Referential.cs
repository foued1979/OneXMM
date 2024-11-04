using System;

namespace OneXMM.Configuration
{
    using OneXMM.Referential.Assets;

    internal sealed class Referential
    {
        public Coin[] Coins { get; set; } = [];
        public PairDef[] PairDefs { get; set; } = [];
        public Addresses TokenAddresses { get; set; }
        public PairDef GetPairDef(Pair pair) => PairDefs.First(pd => pd.CoinId == pair.Coin.Id && pd.BaseId == pair.Base.Id);
    }
}
