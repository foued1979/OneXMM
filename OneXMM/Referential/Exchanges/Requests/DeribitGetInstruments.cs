using System;

namespace OneXMM.Referential.Exchanges.Requests
{
    using OneXMM.Referential.Exchanges.Instruments;

    internal class DeribitGetInstruments
    {
        public bool testnet { get; set; }
        public DeribitOptionDef[] result { get; set; } = [];

        public DeribitGetInstruments() { }
    }
}
