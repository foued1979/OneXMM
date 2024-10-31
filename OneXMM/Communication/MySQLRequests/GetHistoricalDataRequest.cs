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

        public override HistoricalResult[] GetResults()
        {
            var res = new List<HistoricalResult>();

            while (_reader.Read())
            {
                res.Add(new HistoricalResult() { Symbol = _reader.GetString(0), Timestamp = _reader.GetDateTime(1), Value = _reader.GetDouble(2) });
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
