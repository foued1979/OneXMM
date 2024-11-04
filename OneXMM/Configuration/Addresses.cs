using System;

namespace OneXMM.Configuration
{
    internal struct Addresses
    {
        public SortedDictionary<Blockchains, string> Test { get; set; }
        public SortedDictionary<Blockchains, string> Live { get; set; }
    }
}
