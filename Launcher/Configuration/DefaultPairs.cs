using System;

namespace Launcher.Configuration
{
    using OneXMM;

    internal sealed class DefaultPairs
    {
        public PairSetting[] Pairs { get; set; }
    }

    internal struct PairSetting
    {
        public string Symbol { get; set; }
        public Blockchains Chain { get; set; }
        public KycChoices Kyc { get; set; }
    }
}
