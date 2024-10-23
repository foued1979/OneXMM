using System;

namespace OneXMM.Configuration
{
    internal sealed class PoolUpdateCfg
    {
        public Dictionary<VolUpdateTempos, string[]> TermByUpdateFreq { get; set; } = new Dictionary<VolUpdateTempos, string[]>();
    }
}
