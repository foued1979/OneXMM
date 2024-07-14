using System;

namespace OneXMM.Configuration
{
    public sealed class Settings
    {
        public Dictionary<string, ExchangeConfig> Exchanges { get; set; } = [];
    }
}
