using System;

namespace OneXMM.Configuration
{
    public sealed class ExchangeSettings
    {
        private double _maxExpiry;
        public double MaxEpiry
        {
            get { return _maxExpiry; }
            set { _maxExpiry = value / 365.0; }
        }
        public Dictionary<Exchanges, ExchangeConfig> Exchanges { get; set; } = [];
        public Dictionary<DataFeeders, DataFeederConfig> DataFeeders { get; set; } = [];
    }
}
