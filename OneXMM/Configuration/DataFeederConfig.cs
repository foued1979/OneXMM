using System;

namespace OneXMM.Configuration
{
    public sealed class DataFeederConfig
    {
        public string RPCUri { get; set; } = "";
        public string? APIKey { get; set; } = "";
        public string? DefaultQuotes { get; set; } = "";
        public string ReferenceCurrency { get; set; } = "USD";
        public int Fequency { get; set; } = 1000;
    }
}
