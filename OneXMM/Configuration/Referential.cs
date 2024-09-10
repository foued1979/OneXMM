using System;

namespace OneXMM.Configuration
{
    using OneXMM.Referential.Assets;

    internal sealed class Referential
    {
        public Coin[] Coins { get; set; } = [];
        public PairDef[] PairDefs { get; set; } = [];
    }
}
