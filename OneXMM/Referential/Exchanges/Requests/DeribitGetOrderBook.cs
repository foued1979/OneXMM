using System;

namespace OneXMM.Referential.Exchanges.Requests
{
    using OneXMM.Referential.Exchanges.Responses;

    internal class DeribitGetOrderBook
    {
        public bool testnet { get; set; }
        public DeribitOrderBookDef result { get; set; }

        public DeribitGetOrderBook() { }
    }
}
