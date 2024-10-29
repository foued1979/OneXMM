using System;
using System.Text;
using MySql.Data.MySqlClient;

namespace OneXMM.Communication.MySQLRequests
{
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    internal sealed class GetFuturesRequest : MySQLRequest<FuturesResult>
    {
        public DateTime Asof { get; }
        public Pair Pair { get; }
        
        public GetFuturesRequest(DateTime asof, Pair pair) : base(MySQLTables.futures_ref.ToString())
        {
            Asof = asof;
            Pair = pair;
        }

        public override FuturesResult[] GetResults(MySqlDataReader reader)
        {
            var res = new List<FuturesResult>();

            while (reader.Read())
            {
                res.Add(new FuturesResult() 
                { 
                    Id = reader.GetInt32(0), 
                    Symbol = reader.GetString(1), 
                    UndlSymbol = reader.GetString(2), 
                    Expiry = reader.GetString(3), 
                    Exchange = ExchangeServices.GetExchange(reader.GetString(4).ToEnum<Exchanges>()) });
            }

            return res.ToArray();
        }

        public override string ToCmd()
        {
            return $"SELECT * FROM {TableName} WHERE undl='{Pair.Symbol}' AND inputdate >= '{Asof.ToString(MySQLConnector.DateFormat)}'";
        }
    }
}
