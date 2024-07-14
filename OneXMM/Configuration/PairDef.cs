using System;

namespace OneXMM.Configuration
{
    public sealed class PairDef
    {
        public string Coin { get; set; } = "";
        public string Base { get; set; } = "";

        public Dictionary<Exchanges, string> ExchangeSymbols { get; set; } = new Dictionary<Exchanges, string>();
    }
}
