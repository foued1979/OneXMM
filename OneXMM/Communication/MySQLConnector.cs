using OneXMM.Communication.MySQLRequests;
using System;

namespace OneXMM.Communication
{
    internal sealed class MySQLConnector
    {
        public const string DateFormat = "yyyy-MM-dd";
        public const string DatetimeFormat = "yyyy-MM-dd HH:mm:ss";

        private MySql.Data.MySqlClient.MySqlConnection _connection;

        public MySQLConnector(string user, string pwd, string marketdataDB, int port)
        {
            var connectionString = $"server=127.0.0.1;port={port};uid={user};pwd={pwd};database={marketdataDB}";
            _connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString);
            _connection.Open();

            var tableRequest = new GetTablesRequest();

            using (var cmd = _connection.CreateCommand())
            {
                cmd.CommandText = tableRequest.ToCmd();

                using (var reader = cmd.ExecuteReader())
                {
                    var tables = tableRequest.GetResults(reader);
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
                    return request.GetResults(reader);
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
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void CreateTables(MySql.Data.MySqlClient.MySqlConnection connection, MySQLTables table)
        {
            using (var cmd = connection.CreateCommand())
            {
                switch (table)
                {
                    case MySQLTables.historical_prices:
                        cmd.CommandText = $"CREATE TABLE {MySQLTables.historical_prices.ToString()}(symbol varchar(30) not null, date datetime not null, price double not null, CONSTRAINT unique_quote UNIQUE (symbol,date));";
                        break;
                    case MySQLTables.futures_ref:
                        cmd.CommandText = $"CREATE TABLE {MySQLTables.futures_ref.ToString()}(id int not null auto_increment, symbol varchar(40) not null, undl varchar(20) not null, expiry string not null, exchange varchar(20) not null, inputdate datetime not null, CONSTRAINT unique_futures UNIQUE (symbol,exchange));";
                        break;
                }
                
                cmd.ExecuteNonQuery();
            }
        }
    }
}
