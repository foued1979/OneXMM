using System;

namespace OneXMM.Communication.MySQLRequests
{
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Referential.Assets;

    public sealed class GetVolSurfaceRequest : MySQLRequest<VolSurfaceResult>
    {
        public DateTime Asof { get; }
        public Pair Pair { get; }

        public GetVolSurfaceRequest(DateTime asof, Pair pair) : base(MySQLTables.vol_surfaces.ToString())
        {
            Asof = asof;
            Pair = pair;
        }

        public override VolSurfaceResult[] GetResults()
        {
            var res = new VolSurfaceResult[1];

            while (_reader.Read())
            {
                res[0] = new VolSurfaceResult() { Symbol = _reader.GetString(0), Timestamp = _reader.GetDateTime(1), Json = _reader.GetString(2) };
                break;
            }

            return res;
        }

        public override string ToCmd()
        {
            return $"SELECT t1.* FROM {TableName} t1 WHERE t1.timestamp=(" +
                $"SELECT MAX(t2.timestamp) FROM {TableName} t2 WHERE t2.symbol='{Pair.Symbol}' AND t2.timestamp <= '{Asof.ToString(MySQLConnector.DateFormat)}'" +
                $");";
        }
    }
}
