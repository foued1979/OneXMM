using System;

namespace OneXMM.Communication.MySQLRequests
{
    internal abstract class MySQLRequest<TResult> : MySQLRequest
        where TResult : struct
    {
        public string TableName { get; }

        public MySQLRequest(string tableName)
        {
            TableName = tableName;
        }

        public abstract TResult[] GetResults(MySql.Data.MySqlClient.MySqlDataReader reader);
    }

    internal abstract class MySQLRequest
    {
        public abstract string ToCmd();
    }
}
