using OneXMM.Communication.MySQLRequests;
using System;

namespace OneXMM.Communication
{
    internal sealed class MySQLConnector
    {
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
                    if (tableRequest.GetResults(reader).Count() == 0) 
                    { 
                        reader.Close(); 
                        CreateTables(_connection); 
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

        public static void CreateTables(MySql.Data.MySqlClient.MySqlConnection connection)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"CREATE TABLE {MySQLTables.historical_prices.ToString()}(symbol varchar(30) not null, date Datetime not null, price double not null, CONSTRAINT unique_quote UNIQUE (symbol,date));";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
