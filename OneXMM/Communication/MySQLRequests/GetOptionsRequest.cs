using System;

namespace OneXMM.Communication.MySQLRequests
{
    using MySql.Data.MySqlClient;
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Referential.Assets;
    using OneXMM.Services;
    using OneXMM.Utils;

    internal sealed class GetOptionsRequest : MySQLRequest<OptionResult>
    {
        public DateTime Asof { get; }
        public Pair Pair { get; }
        public DateTime LatestInputDate { get; private set; }

        public GetOptionsRequest(DateTime asof, Pair pair) : base(MySQLTables.options_ref.ToString())
        {
            Asof = asof;
            Pair = pair;
        }

        public override OptionResult[] GetResults()
        {
            var res = new List<OptionResult>();

            while (_reader.Read())
            {
                // We check the last update
                // This is used later to check whether Today has been updated
                var inputDate = _reader.GetDateTime(5);
                LatestInputDate = inputDate.CompareTo(LatestInputDate) > 0 ? inputDate : LatestInputDate;

                res.Add(new OptionResult()
                {
                    Id = _reader.GetInt32(0),
                    Symbol = _reader.GetString(1),
                    OptionType = _reader.GetString(2).ToEnum<OptionTypes>(),
                    Strike = _reader.GetDouble(3),
                    UndlSymbol = _reader.GetString(4),
                    Expiry = _reader.GetDateTime(5),
                    Exchange = ExchangeService.GetExchange(_reader.GetString(6).ToEnum<Exchanges>())
                });
            }

            return res.ToArray();
        }

        public override string ToCmd()
        {
            return $"SELECT * FROM {TableName} WHERE undl='{Pair.Symbol}' AND expiry >= '{Asof.ToString(MySQLConnector.DateFormat)}'";
        }
    }
}
