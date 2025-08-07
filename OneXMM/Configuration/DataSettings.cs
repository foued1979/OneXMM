using System;

namespace OneXMM.Configuration
{
    public class DataSettings
    {
        public DataFeeders[] Feeders { get; set; } = [];
        public Exchanges[] Exchanges { get; set; } = [];
    }
}
