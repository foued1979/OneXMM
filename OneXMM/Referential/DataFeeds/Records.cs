using System;

namespace OneXMM.Referential.DataFeeds
{
    using OneXMM.Referential.Assets;

    public record RawQuote(int Id, double Price);
    public record Quote(Pair Pair, double Price);
    public record HistoricalQuotes(Coin Coin, HistoricalQuote[] Quotes);
    public record HistoricalQuote(DateTime Timestamp, double Price);
}
