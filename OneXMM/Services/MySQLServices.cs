using System;
using System.Runtime.CompilerServices;

namespace OneXMM.Services
{
    using OneXMM.Communication.MySQLRequests;
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Communication;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InsertData(InsertDataRequest insertData)
        {
            _sqlConnector.InsertData(insertData);
        }
    }
}
