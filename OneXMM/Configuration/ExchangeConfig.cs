using System;

namespace OneXMM.Configuration
{
    public sealed class ExchangeConfig
    {
        public string ExchangeName { get; set; } = "";
        public string TestRPCUri { get; set; } = "";
        public string ProdRPCUri { get; set; } = "";
        public string TestWsUri { get; set; } = "";
        public string ProdWsUri { get; set; } = "";
        public string Cutoff { get; set; } = "";
        public bool? UseUniversalTime { get; set; }
        public string ExpiryFormat { get; set; } = "";
        public bool OptionsAreDiscounted { get; set; }
        public bool OptionsAreQuotedInBase { get; set; }
    }
}
