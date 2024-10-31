using System;

namespace OneXMM.Communication.MySQLRequests
{
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    public sealed class GetFuturesRequest : MySQLRequest<FuturesResult>
    {
        public DateTime Asof { get; }
        public Pair Pair { get; }


        public GetFuturesRequest(DateTime asof, Pair pair) : base(MySQLTables.futures_ref.ToString())
        {
            Asof = asof;
            Pair = pair;
        }

        public override FuturesResult[] GetResults()
        {
            var res = new List<FuturesResult>();

            while (_reader.Read())
            {
                res.Add(new FuturesResult() 
                { 
                    Id = _reader.GetInt32(0), 
                    Symbol = _reader.GetString(1), 
                    UndlSymbol = _reader.GetString(2), 
                    Expiry = _reader.GetString(3), 
                    Exchange = ExchangeServices.GetExchange(_reader.GetString(4).ToEnum<Exchanges>()) });
            }

            return res.ToArray();
        }

        public override string ToCmd()
        {
            return $"SELECT * FROM {TableName} WHERE undl='{Pair.Symbol}' AND inputdate >= '{Asof.ToString(MySQLConnector.DateFormat)}'";
        }
    }
}
