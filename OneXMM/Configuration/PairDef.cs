using System;

namespace OneXMM.Configuration
{
    public sealed class PairDef
    {
        public int CoinId { get; set; } = -1;
        public int BaseId { get; set; } = -1;
        public Exchanges RefExchange { get; set; } = Exchanges.None;
        public PoolSettings[] PoolSettings { get; set; } = [];
        public bool CorrelatedVol { get; set; } = false;
    }

    public struct PoolSettings
    {
        public string Term { get; set; }
        public FixingIntervals FixingInterval { get; set; }
    }
}
