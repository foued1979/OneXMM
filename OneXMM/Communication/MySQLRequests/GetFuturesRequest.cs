using System;

namespace OneXMM.Communication.MySQLRequests
{
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Referential.Assets;
    using OneXMM.Services;
    using OneXMM.Utils;

    public sealed class GetFuturesRequest : MySQLRequest<FuturesResult>
    {
        public DateTime Asof { get; }
        public Pair Pair { get; }
        public DateTime LatestInputDate { get; private set; }


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
                // We check the last update
                // This is used later to check whether Today has been updated
                var inputDate = _reader.GetDateTime(5);
                LatestInputDate = inputDate.CompareTo(LatestInputDate) > 0 ? inputDate : LatestInputDate;

                res.Add(new FuturesResult() 
                { 
                    Id = _reader.GetInt32(0), 
                    Symbol = _reader.GetString(1), 
                    UndlSymbol = _reader.GetString(2), 
                    Expiry = _reader.GetDateTime(3), 
                    Exchange = ExchangeService.GetExchange(_reader.GetString(4).ToEnum<Exchanges>()) });
            }

            return res.ToArray();
        }

        public override string ToCmd()
        {
            return $"SELECT * FROM {TableName} WHERE undl='{Pair.Symbol}' AND expiry >= '{Asof.ToString(MySQLConnector.DateFormat)}'";
        }
    }
}
