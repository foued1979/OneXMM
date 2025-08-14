using System;

namespace OneXMM.Referential.DataFeeds
{
    using OneXMM.Referential.Assets;

    public record RawQuote(int Id, double Price);
    public record Quote(Pair Pair, double Price);
    public record HistoricalQuotes(Coin Coin, Coin Base, HistoricalQuote[] Quotes);
    public record HistoricalQuote(DateTime Timestamp, double Price);

    public static class RecordOperations
    {
        public static HistoricalQuotes Divide(HistoricalQuotes num, HistoricalQuotes denom)
        {
            if (num.Quotes.Length != denom.Quotes.Length) throw new Exception("HistoricalQuotes division: not same number of data.");
            var isMult = num.Base.Equals(denom.Coin);
            if (!num.Base.Equals(denom.Base) || !isMult) throw new Exception("Cannot divide historical quotes which are not related.");

            var newQuotes = new HistoricalQuote[num.Quotes.Length];
            var numQuotes = num.Quotes;
            var denomQuotes = denom.Quotes;

            for (int i = 0; i < newQuotes.Length; ++i)
            {
                var numQuote = newQuotes[i];
                var denomQuote = denomQuotes[i];
                if (numQuote.Timestamp.CompareTo(denomQuote.Timestamp) != 0) throw new Exception("HistoricalQuotes division: data not consistent.");

                newQuotes[i] = new HistoricalQuote(numQuote.Timestamp, isMult ? numQuote.Price * denomQuote.Price : numQuote.Price / denomQuote.Price);
            }

            return new HistoricalQuotes(num.Coin, isMult ? denom.Base : denom.Coin, newQuotes);
        }
    }
}
