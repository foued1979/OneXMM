using System;

namespace OneXMM.Configuration
{
    public sealed class PairDef
    {
        public int Id { get; set; } = -1;
        public int CoinId { get; set; } = -1;
        public int BaseId { get; set; } = -1;
        public Exchanges RefExchange { get; set; } = Exchanges.None;
        public Dictionary<KycStatus, PoolSetting[]> PoolSettings { get; set; } = new Dictionary<KycStatus, PoolSetting[]>(2);
        public bool CorrelatedVol { get; set; } = false;
    }

    public struct PoolSetting
    {
        public string Term { get; set; }
        public FixingIntervals FixingInterval { get; set; }
        internal string TestAddress { get; set; }
        internal string LiveAddress { get; set; }
    }
}
