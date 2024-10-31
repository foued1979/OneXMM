using MySql.Data.MySqlClient;
using System;
using System.Text;

namespace OneXMM.Communication
{
    using OneXMM.Communication.MySQLRequests;
    using OneXMM.Interfaces;

    internal sealed class MySQLConnector : IDBConnector
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const string DatetimeFormat = "yyyy-MM-dd HH:mm:ss";

        private MySqlConnection _connection;

        public MySQLConnector(string user, string pwd, string marketdataDB, int port)
        {
            var connectionString = $"server=127.0.0.1;port={port};uid={user};pwd={pwd};database={marketdataDB}";
            _connection = new MySqlConnection(connectionString);
            _connection.Open();

            var tableRequest = new GetTablesRequest();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = tableRequest.ToCmd();

                using (var reader = cmd.ExecuteReader())
                {
                    var tables = tableRequest.WithReader(reader).GetResults();
                    reader.Close();

                    foreach (MySQLTables table in Enum.GetValues(typeof(MySQLTables)))
                    {
                        if (!tables.Any(t => t.TableName.Equals(table.ToString()))) CreateTables(_connection, table);
                    }
                }
            }
        }

        public IEnumerable<TResult> Get<TResult>(MySQLRequest<TResult> request)
            where TResult : struct
        {
            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = request.ToCmd();
                using (var reader = cmd.ExecuteReader())
                    return request.WithReader(reader).GetResults();
            }
        }

        public void InsertData(InsertDataRequest insertRequest)
        {
            var cmds = insertRequest.ToCmd();
            for (int i = 0; i < cmds.Length; ++i)
            {
                using (var cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = cmds[i];

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (MySqlException ex)
                    {
                        if (ex.Number == 1062) continue;
                        else throw new Exception("MySQL insert error", ex);
                    }
                }
            }
        }

        internal static void CreateTables(MySql.Data.MySqlClient.MySqlConnection connection, MySQLTables table)
        {
            using (var cmd = connection.CreateCommand())
            {
                var sb = new StringBuilder();
                var isFirstField = true;

                switch (table)
                {
                    case MySQLTables.historical_prices:
                        sb.Append($"CREATE TABLE {MySQLTables.historical_prices.ToString()} (");
                        foreach (var field in TableDef.HistoricalPrices)
                        {
                            if (isFirstField) { sb.Append($"{field.ColumnName} {field.Definition}"); isFirstField = false; }
                            else sb.Append($",{field.ColumnName} {field.Definition}");
                        }
                            
                        sb.Append(",CONSTRAINT unique_quote UNIQUE(symbol, date));");
                        cmd.CommandText = sb.ToString();
                        break;
                    case MySQLTables.futures_ref:
                        sb.Append($"CREATE TABLE {MySQLTables.futures_ref.ToString()} (");
                        foreach (var field in TableDef.FuturesRef)
                        {
                            if (isFirstField) { sb.Append($"{field.ColumnName} {field.Definition}"); isFirstField = false; }
                            else sb.Append($",{field.ColumnName} {field.Definition}");
                        }
                        sb.Append(",PRIMARY KEY (id),CONSTRAINT unique_futures UNIQUE (symbol,exchange));");
                        cmd.CommandText = sb.ToString();
                        break;
                    case MySQLTables.options_ref:
                        sb.Append($"CREATE TABLE {MySQLTables.options_ref.ToString()} (");
                        foreach (var field in TableDef.OptionsRef)
                        {
                            if (isFirstField) { sb.Append($"{field.ColumnName} {field.Definition}"); isFirstField = false; }
                            else sb.Append($",{field.ColumnName} {field.Definition}");
                        }
                        sb.Append(",PRIMARY KEY (id),CONSTRAINT unique_option UNIQUE (symbol,exchange));");
                        cmd.CommandText = sb.ToString();
                        break;
                    case MySQLTables.transactions:
                        sb.Append($"CREATE TABLE {MySQLTables.transactions.ToString()} (");
                        foreach (var field in TableDef.Transactions)
                        {
                            if (isFirstField) { sb.Append($"{field.ColumnName} {field.Definition}"); isFirstField = false; }
                            else sb.Append($",{field.ColumnName} {field.Definition}");
                        }
                        sb.Append(",PRIMARY KEY (transactionHash));");
                        cmd.CommandText = sb.ToString();
                        break;
                }
                
                cmd.ExecuteNonQuery();
            }
        }
    }
}
