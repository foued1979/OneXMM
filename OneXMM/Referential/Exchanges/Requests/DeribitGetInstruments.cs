using System;

namespace OneXMM.Referential.Exchanges.Requests
{
    using OneXMM.Referential.Exchanges.Responses;

    internal class DeribitGetInstruments
    {
        public bool testnet { get; set; }

        public DeribitInstrumentDef[] result { get; set; } = [];

        public DeribitGetInstruments() { }
    }
}
