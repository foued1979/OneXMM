using System;

namespace OneXMM.Communication.MySQLRequests
{
    using MySql.Data.MySqlClient;

    public abstract class MySQLRequest<TResult> : MySQLRequest
        where TResult : struct
    {
        protected MySqlDataReader _reader;

        public string TableName { get; }

        public MySQLRequest(string tableName)
        {
            TableName = tableName;
        }

        public abstract TResult[] GetResults();

        internal MySQLRequest<TResult> WithReader(MySql.Data.MySqlClient.MySqlDataReader reader)
        {
            _reader = reader;
            return this;
        }
    }

    public abstract class MySQLRequest
    {
        public abstract string ToCmd();
    }
}
