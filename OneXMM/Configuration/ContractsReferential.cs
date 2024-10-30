using System;

namespace OneXMM.Configuration
{
    internal sealed class ContractsReferential
    {
        public ContractMapping Test { get; set; }
        public ContractMapping Live { get; set; }
    }

    internal sealed class  ContractMapping
    {
        public Dictionary<Blockchains, string> OneXMM { get; set; }
        public Dictionary<Blockchains, Dictionary<string, Dictionary<string, string>>> Pools { get; set; }
    }
}
