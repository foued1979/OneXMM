using System;
using System.Runtime.CompilerServices;

namespace OneXMM.Services
{
    using OneXMM.Communication.MySQLRequests;
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Communication;
    using OneXMM.Referential.DataFeeds;

    internal static class MySQLService
    {
        private readonly static IDBConnector _sqlConnector = Factory.CreateMySQLConnector();

        public static void InsertQuote(HistoricalResult quote)
        {
            _sqlConnector.InsertData(new InsertDataRequest(MySQLTables.historical_prices.ToString(),
                [[quote.Symbol, quote.Timestamp.ToString(MySQLConnector.DatetimeFormat), quote.Value.ToString()]],
                MySQLInsertTypes.None));
        }

        public static void InsertQuotes(HistoricalQuotes quotes)
        {
            var symbol = $"{quotes.Coin.ContractSymbol}{Pair.SymbolSeparator}{quotes.Base.ContractSymbol}";
            string[][] dataToInsert = new string[quotes.Quotes.Length][];

            for (int i = 0; i < quotes.Quotes.Length; ++i)
            {
                var quote = quotes.Quotes[i];
                dataToInsert[i] = [symbol, quote.Timestamp.ToString(MySQLConnector.DatetimeFormat), quote.Price.ToString()];
            }

            _sqlConnector.InsertData(new InsertDataRequest(MySQLTables.historical_prices.ToString(),
                dataToInsert,
                MySQLInsertTypes.None));
        }

        public static IEnumerable<HistoricalResult> GetHistoricalQuote(Pair pair, DateTime? date = null)
        {
            return _sqlConnector.Get(new GetHistoricalDataRequest(MySQLTables.historical_prices.ToString()) { Symbol = pair.Symbol, Date = date });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InsertData(InsertDataRequest insertData)
        {
            _sqlConnector.InsertData(insertData);
        }
    }
}
