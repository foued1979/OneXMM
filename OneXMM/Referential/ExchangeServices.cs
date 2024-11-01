using System;

using System.Runtime.CompilerServices;

namespace OneXMM.Referential
{
    using OneXMM.Communication;
    using OneXMM.Communication.MySQLRequests;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges;

    public static class ExchangeServices
    {
        public static bool IsInitiated { get; private set; } = false;

        private readonly static IDBConnector _sqlConnector = Factory.CreateMySQLConnector();
        internal readonly static SortedDictionary<OneXMM.Exchanges, Exchange> _exchanges;
        internal readonly static SortedDictionary<Pair, (Futures[], Option[])> _instrumentsByPair;

        public static void Init() { IsInitiated = true; }

        static ExchangeServices()
        {
            _exchanges = new SortedDictionary<OneXMM.Exchanges, Exchange>();

            foreach (OneXMM.Exchanges exchange in Enum.GetValues(typeof(OneXMM.Exchanges)))
            {
                if (exchange == OneXMM.Exchanges.None) continue;
                AddExchange(exchange);
            }

            _instrumentsByPair = new SortedDictionary<Pair, (Futures[], Option[])>();
            var today = DateTime.Today;
            var todayStr = today.ToString(MySQLConnector.DateFormat);

            foreach (var pair in Config.GetPairs())
            {
                // For correlated vol, there is no instrument to load for the pair
                if (pair.CorrelatedVol) continue;

                var exchange = _exchanges[Config.GetReferenceExchangeForPair(pair)];

                var getFuturesRequest = new GetFuturesRequest(today, pair);
                var futuresResult = _sqlConnector.Get(getFuturesRequest);
                Futures[]? futures = null;

                if (futuresResult.Count() == 0 || getFuturesRequest.LatestInputDate.CompareTo(today) < 0)
                {
                    futures = exchange.LoadFutures(pair);
                    string[][] dataToInsert = new string[futures.Length][];

                    for (int i = 0; i < futures.Length; ++i)
                    {
                        dataToInsert[i] = futures[i].ToFields();
                        dataToInsert[i] = dataToInsert[i].Append(todayStr).ToArray();
                    }

                    var insertReq = new InsertDataRequest(MySQLTables.futures_ref.ToString(), dataToInsert, MySQLInsertTypes.IdNotInDataToInsert);
                    _sqlConnector.InsertData(insertReq);
                }
                else
                {
                    futures = futuresResult.Select(r => r.ToFutures(pair)).ToArray();
                }

                var getOptionRequest = new GetOptionsRequest(today, pair);
                var optionsResult = _sqlConnector.Get(getOptionRequest);
                Option[]? options = null;

                if (optionsResult.Count() == 0 || getOptionRequest.LatestInputDate.CompareTo(today) < 0)
                {
                    options = exchange.LoadOptions(pair);
                    string[][] dataToInsert = new string[options.Length][];

                    for (int i = 0; i < options.Length; ++i)
                    {
                        dataToInsert[i] = options[i].ToFields();
                        dataToInsert[i] = dataToInsert[i].Append(todayStr).ToArray();
                    }

                    var insertReq = new InsertDataRequest(MySQLTables.options_ref.ToString(), dataToInsert, MySQLInsertTypes.IdNotInDataToInsert);
                    _sqlConnector.InsertData(insertReq);
                }
                else
                {
                    options = optionsResult.Select(r => r.ToOption(pair)).ToArray();
                }

                _instrumentsByPair.Add(pair, (futures, options));
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
            foreach (var pair in pairs)
                UpdateOptionAndFuturesReferential(pair);
        }

        public static void UpdateOptionAndFuturesReferential(Pair pair)
        {
            // For correlated vol, there is no instrument to load for the pair
            if (pair.CorrelatedVol) return;

            var today = DateTime.Today;
            var todayStr = today.ToString(MySQLConnector.DateFormat);

            var exchange = _exchanges[Config.GetReferenceExchangeForPair(pair)];

            // We load the new futures
            var futures = exchange.LoadFutures(pair);
            string[][] dataToInsert = new string[futures.Length][];

            for (int i = 0; i < futures.Length; ++i)
            {
                dataToInsert[i] = futures[i].ToFields();
                dataToInsert[i] = dataToInsert[i].Append(todayStr).ToArray();
            }

            var insertReq = new InsertDataRequest(MySQLTables.futures_ref.ToString(), dataToInsert, MySQLInsertTypes.IdNotInDataToInsert);
            _sqlConnector.InsertData(insertReq);
            // --------

            // We load the new options
            var options = exchange.LoadOptions(pair);
            dataToInsert = new string[options.Length][];

            for (int i = 0; i < options.Length; ++i)
            {
                dataToInsert[i] = options[i].ToFields();
                dataToInsert[i] = dataToInsert[i].Append(todayStr).ToArray();
            }

            insertReq = new InsertDataRequest(MySQLTables.options_ref.ToString(), dataToInsert, MySQLInsertTypes.IdNotInDataToInsert);
            _sqlConnector.InsertData(insertReq);

            lock (_instrumentsByPair)
                if (_instrumentsByPair.TryGetValue(pair, out var instrs))
                {
                    _instrumentsByPair[pair] = (futures, options);
                }
                else
                {
                    _instrumentsByPair.Add(pair, (futures, options));
                }
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
