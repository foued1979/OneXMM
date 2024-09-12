using System;

namespace OneXMM.Referential.Exchanges.Requests
{
    using OneXMM.Referential.Exchanges.Responses;

    internal class DeribitGetIndex
    {
        public bool testnet { get; set; }
        public DeribitIndexDef result { get; set; }

        public DeribitGetIndex() { }
    }
}
