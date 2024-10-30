using System;

namespace OneXMM.Communication.MySQLRequests
{
    using MySql.Data.MySqlClient;
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    internal sealed class GetOptionsRequest : MySQLRequest<OptionResult>
    {
        public DateTime Asof { get; set; }
        public Pair Pair { get; set; }

        public GetOptionsRequest(DateTime asof, Pair pair) : base(MySQLTables.options_ref.ToString())
        {
            Asof = asof;
            Pair = pair;
        }

        public override OptionResult[] GetResults(MySqlDataReader reader)
        {
            var res = new List<OptionResult>();

            while (reader.Read())
            {
                res.Add(new OptionResult()
                {
                    Id = reader.GetInt32(0),
                    Symbol = reader.GetString(1),
                    OptionType = reader.GetString(2).ToEnum<OptionTypes>(),
                    Strike = reader.GetDouble(3),
                    UndlSymbol = reader.GetString(4),
                    Expiry = reader.GetString(5),
                    Exchange = ExchangeServices.GetExchange(reader.GetString(6).ToEnum<Exchanges>())
                });
            }

            return res.ToArray();
        }

        public override string ToCmd()
        {
            return $"SELECT * FROM {TableName} WHERE undl='{Pair.Symbol}' AND inputdate >= '{Asof.ToString(MySQLConnector.DateFormat)}'";
        }
    }
}
