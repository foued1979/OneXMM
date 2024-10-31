using System;
using MySql.Data.MySqlClient;

namespace OneXMM.Communication.MySQLRequests
{
    using OneXMM.Communication.MySQLResults;

    internal class GetTablesRequest : MySQLRequest<TableResult>
    {
        public GetTablesRequest() : base("") { }

        public override TableResult[] GetResults()
        {
            var res = new List<TableResult>();

            while (_reader.Read())
            {
                res.Add(new TableResult() { TableName = _reader.GetString(0) });
            }

            return res.ToArray();
        }

        public override string ToCmd()
        {
            return "SHOW TABLES;";
        }
    }
}
