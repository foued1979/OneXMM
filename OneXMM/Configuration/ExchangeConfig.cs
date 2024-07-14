using System;

namespace OneXMM.Configuration
{
    public sealed class ExchangeConfig
    {
        public string ExchangeName { get; set; } = "";
        public string TestUri { get; set; } = "";
        public string ProdUri { get; set; } = "";
        public string Cutoff { get; set; } = "";
        public string ExpiryFormat { get; set; } = "";
    }
}
