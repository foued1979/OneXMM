using System;

namespace OneXMM.Communication.Services
{
    using OneXMM.Communication.MySQLRequests;
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;

    internal static class MySQLServices
    {
        private readonly static IDBConnector _sqlConnector = Factory.CreateMySQLConnector();

        public static void InsertQuote(HistoricalResult quote)
        {
            _sqlConnector.InsertData(new InsertDataRequest(MySQLTables.historical_prices.ToString(),
                [[quote.Symbol, quote.Timestamp.ToString(MySQLConnector.DatetimeFormat), quote.Value.ToString()]],
                MySQLInsertTypes.None));
        }

        public static IEnumerable<HistoricalResult> GetHistoricalQuote(Pair pair, DateTime? date = null)
        {
            return _sqlConnector.Get(new GetHistoricalDataRequest(MySQLTables.historical_prices.ToString()) { Symbol = pair.Symbol, Date = date });
        }
    }
}
