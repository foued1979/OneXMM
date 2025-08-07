using System;

namespace OneXMM.Referential.DataFeeds.Responses
{
    internal struct CoinMarketCapHistoricalQuotes
    {
        public Dictionary<int, HistoricalQuotes> data { get; set; }
    }

    internal struct CoinMarketCapHistoricalQuote
    {
        public HistoricalQuotes data { get; set; }
    }

    internal struct HistoricalQuotes
    {
        public HistoricalQuote[] quotes  { get; set; }
    }

    internal struct HistoricalQuote
    {
        public DateTime timestamp { get; set; }
        public Dictionary<string, QuoteDetails> quote { get; set; }
    }

    internal struct QuoteDetails
    {
        public decimal price { get; set; }
    }
}
