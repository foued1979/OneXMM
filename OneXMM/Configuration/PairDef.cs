using System;

namespace OneXMM.Configuration
{
    public sealed class PairDef
    {
        public int CoinId { get; set; } = -1;
        public int BaseId { get; set; } = -1;
        public Exchanges RefExchange { get; set; } = Exchanges.None;
        public Dictionary<KycStatus, Term[]> PoolSettings { get; set; } = new Dictionary<KycStatus, Term[]>(2);
        public bool CorrelatedVol { get; set; } = false;
    }

    public struct Term
    {
        public string TermValue { get; set; }
        public FixingIntervals FixingInterval { get; set; }
    }
}
