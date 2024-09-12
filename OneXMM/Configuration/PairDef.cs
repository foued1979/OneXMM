using System;

namespace OneXMM.Configuration
{
    public sealed class PairDef
    {
        public string Coin { get; set; } = "";
        public string Base { get; set; } = "";
        public Blockchains Chain { get; set; } = Blockchains.Error;
    }
}
