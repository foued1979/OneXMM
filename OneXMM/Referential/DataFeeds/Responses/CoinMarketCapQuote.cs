using System;

namespace OneXMM.Referential.DataFeeds.Responses
{
    internal struct CoinMarketCapQuote
    {
        public Dictionary<int, Details> data { get; set; }
    }

    internal struct Details
    {
        public Dictionary<string, Price> quote { get; set; }
    }

    internal struct Price
    {
        public decimal price { get; set; }
    }
}
