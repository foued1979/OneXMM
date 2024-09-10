using System;

namespace OneXMM.Configuration
{
    public sealed class ExchangeSettings
    {
        public Dictionary<Exchanges, ExchangeConfig> Exchanges { get; set; } = [];
    }
}
