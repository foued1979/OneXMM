using System;

namespace OneXMM.Configuration
{
    internal sealed class SymbolMapping
    {
        public SortedDictionary<string, Dictionary<SymbolTypes, Dictionary<Exchanges, string>>> Mapping { get; set; } = new SortedDictionary<string, Dictionary<SymbolTypes, Dictionary<Exchanges, string>>>();
    }
}
