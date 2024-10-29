using System;

using System.Runtime.CompilerServices;

namespace OneXMM.Referential
{
    using OneXMM.Communication;
    using OneXMM.Communication.MySQLRequests;
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges;

    public static class ExchangeServices
    {
        private readonly static MySQLConnector _sqlConnector = Factory.CreateMySQLConnector();
        internal readonly static SortedDictionary<OneXMM.Exchanges, Exchange> _exchanges;
        internal readonly static SortedDictionary<Pair, (Futures[], Option[])> _instrumentsByPair;

        static ExchangeServices()
        {
            _exchanges = new SortedDictionary<OneXMM.Exchanges, Exchange>();

            foreach (OneXMM.Exchanges exchange in Enum.GetValues(typeof(OneXMM.Exchanges)))
            {
                AddExchange(exchange);
            }

            _instrumentsByPair = new SortedDictionary<Pair, (Futures[], Option[])>();
            var today = DateTime.Today;

            foreach (var pair in Config.GetPairs())
            {
                // For correlated vol, there is no instrument to load for the pair
                if (pair.CorrelatedVol) continue;

                var exchange = _exchanges[Config.GetReferenceExchangeForPair(pair)];

                var getFuturesRequest = new GetFuturesRequest(today, pair);
                var futuresResult = _sqlConnector.Get(getFuturesRequest);
                Futures[]? futures = null;

                if (futuresResult.Count() == 0)
                {
                    futures = exchange.LoadFutures(pair);
                    string[][] dataToInsert = new string[futures.Length][];

                    for (int i = 0; i < futures.Length; ++i)
                    {
                        dataToInsert[i] = futures[i].ToFields();
                    }

                    var insertReq = new InsertDataRequest(MySQLTables.futures_ref.ToString(), dataToInsert);
                    _sqlConnector.InsertData(insertReq);
                }
                else
                {
                    futures = futuresResult.Select(r => r.ToFutures(pair)).ToArray();
                }
                
                

                var options = exchange.LoadOptions(pair);
            }
        }

        public static void AddExchange(OneXMM.Exchanges exchange)
        {
            if (_exchanges.ContainsKey(exchange)) return;

            switch (exchange)
            {
                case OneXMM.Exchanges.Deribit:
                    _exchanges.Add(exchange, new Deribit(Config.IsTestMode));
                    break;
                default:
                    throw new NotImplementedException($"Unknown {exchange}.");
            }
        }

        public static void UpdateOptionAndFuturesReferential(IEnumerable<Pair> pairs)
        {
            ggg
        }

        public static void UpdateOptionAndFuturesReferential(Pair pair)
        {
            ggg
        }

        public static Exchange GetExchange(OneXMM.Exchanges exchange) => _exchanges[exchange];

        /// <summary>
        /// Returns the reference exchange for the pair.
        /// </summary>
        public static Exchange GetExchange(Pair pair)
        {
            var pairDef = Config._referential.PairDefs.First(pd => pd.CoinId == pair.Coin.Id && pd.BaseId == pair.Base.Id);
            return _exchanges[pairDef.RefExchange];
        }

        public static bool GetRefInstrumentsForPair(Pair pair, out (Futures[], Option[]) refInstruments)
        {
            if (_instrumentsByPair.TryGetValue(pair, out refInstruments)) return true;
            refInstruments = ([], []);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetSpot(Pair pair)
        {
            return _exchanges[Config.GetReferenceExchangeForPair(pair)].GetSpot(pair);
        }
    }
}
