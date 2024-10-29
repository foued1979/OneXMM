using System;
using System.Text;
using MySql.Data.MySqlClient;

namespace OneXMM.Communication.MySQLRequests
{
    using OneXMM.Communication.MySQLResults;

    internal sealed class GetHistoricalDataRequest : MySQLRequest<HistoricalResult>
    {
        public string Symbol { get; set; } = "";
        public DateTime? Date { get; set; }

        public GetHistoricalDataRequest(string tableName) : base(tableName) { }

        public override HistoricalResult[] GetResults(MySqlDataReader reader)
        {
            var res = new List<HistoricalResult>();

            while (reader.Read())
            {
                res.Add(new HistoricalResult() { Symbol = reader.GetString(0), Timestamp = reader.GetDateTime(1), Value = reader.GetDouble(2) });
            }

            return res.ToArray();
        }

        public override string ToCmd()
        {
            var sb = new StringBuilder();
            sb.Append($"SELECT * FROM {TableName}");
            var hasFilter = false;

            if (!string.IsNullOrEmpty(Symbol)) { hasFilter = true; sb.Append($" WHERE symbol='{Symbol}'"); }
            if (Date.HasValue)
            {
                if (hasFilter) sb.Append($" AND date='{Date.Value.ToString(MySQLConnector.DatetimeFormat)}'");
                else sb.Append($" WHERE date='{Date.Value.ToString(MySQLConnector.DatetimeFormat)}'");
            }

            sb.Append(";");
            return sb.ToString();
        }
    }
}
