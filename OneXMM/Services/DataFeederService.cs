using System;

namespace OneXMM.Services
{
    using OneXMM.Communication.MySQLRequests;
    using OneXMM.Interfaces;
    using OneXMM.Math.Tools;
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.DataFeeds;
    using OneXMM.Services.ServiceEventArgs;
    using System.Runtime.CompilerServices;

    public static class DataFeederService
    {
        #region Events
        public static event EventHandler<DataMissingEventArgs> DataMissing;
        public static event EventHandler<Quote> QuoteReceived;
        #endregion

        private readonly static IDBConnector _sqlConnector = Factory.CreateMySQLConnector();

        internal readonly static Dictionary<DataFeeders, DataFeeder> _dataFeeders;
        internal readonly static SortedDictionary<Pair, DataFeeder> _mappingPairToFeeder;
        internal readonly static SortedDictionary<Pair, double> _livePrices;
        internal readonly static SortedDictionary<Pair, RollingPriceUpdate> _priceUpdates;

        public static bool IsInitialized { get; private set; } = false;
        public static Pair[] Pairs => _livePrices.Keys.ToArray();

        static DataFeederService()
        {
            var allFeederTypes = Enum.GetValues(typeof(DataFeeders));
            _dataFeeders = new Dictionary<DataFeeders, DataFeeder>(allFeederTypes.Length);
            _mappingPairToFeeder = new SortedDictionary<Pair, DataFeeder>();
            _livePrices = new SortedDictionary<Pair, double>();

            foreach (DataFeeders dataFeeder in allFeederTypes)
            {
                var feeder = AddDataFeeder(dataFeeder);
                feeder.DataMissingForPair += Feeder_DataMissingForPair;
                feeder.QuotesReceived += Feeder_QuotesReceived;

                foreach (var pair in feeder.Quotes)
                {
                    if (!_mappingPairToFeeder.TryAdd(pair, feeder)) throw new Exception($"DataFeederService - Error adding Pair {pair.Id}: pair is already registered.");
                    if (!_livePrices.TryAdd(pair, 0)) throw new Exception($"DataFeederService - Error adding Pair {pair.Id}.");
                }

                var lastQuotes = feeder.GetLatestQuotes().ConfigureAwait(false).GetAwaiter().GetResult();
                foreach (var quote in lastQuotes) _livePrices[Config.GetPairFromFeederId(quote.Id, dataFeeder)] = quote.Price;
            }

            _priceUpdates = new SortedDictionary<Pair, RollingPriceUpdate>();
            UpdateHistoricalQuotes().ConfigureAwait(false).GetAwaiter().GetResult();

            IsInitialized = true;
        }

        public static DataFeeder AddDataFeeder(DataFeeders dataFeeder)
        {
            DataFeeder? feeder = null;

            switch (dataFeeder)
            {
                case DataFeeders.CoinMarketCap:
                    feeder = new CoinMarketCap();
                    _dataFeeders.Add(dataFeeder, feeder);
                    return feeder!;
                default:
                    throw new NotImplementedException($"DataFeederService::AddDataFeeder - Unknown data feeder type {dataFeeder}.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Quote> GetLastQuotes()
        {
            return _livePrices.Select(kv => new Quote(kv.Key, kv.Value));
        }

        public static Quote[] GetLastQuotes(Pair[] pairs)
        {
            var res = new Quote[pairs.Length];
            var index = 0;

            foreach (var pair in pairs)
            {
                var quote = GetLastQuote(pair);
                if (quote is null) throw new Exception($"DataFeederService::GetLastQuotes - Issue getting last quote for pair {pair.Id}.");
                res[index] = quote;

                index++;
            }

            return res;
        }

        public static Quote? GetLastQuote(Pair pair)
        {
            if (!_livePrices.TryGetValue(pair, out var price)) return null;
            return new Quote(pair, price);
        }

        public static void Launch()
        {
            foreach (var feeder in _dataFeeders.Values)
            {
                feeder.GetLiveQuotes();
            }
        }

        public static void Stop()
        {
            foreach (var feeder in _dataFeeders.Values)
            {
                feeder.StopLiveQuotes();
            }
        }

        private static void Feeder_DataMissingForPair(object? sender, int e)
        {
            DataMissing(null, new DataMissingEventArgs(Config.GetPair(e), (sender as DataFeeder)!.FeederType));
        }

        private static void Feeder_QuotesReceived(object? sender, Referential.DataFeeds.Events.QuoteEventArgs e)
        {
            foreach (var quote in e.Quotes)
            {
                QuoteReceived(null, quote);
                _livePrices[quote.Pair] = quote.Price;
                _priceUpdates[quote.Pair].Update(quote.Price);
            }
        }

        private static async Task UpdateHistoricalQuotes()
        {
            foreach (var pair in Pairs)
            {
                var toDate = DateTime.Today.AddDays(-1);
                var fromDate = toDate.AddYears(-1);

                var historicalData = _sqlConnector.Get(new GetHistoricalDataRequest(Config.GetTableName(MySQLTables.historical_prices))
                {
                    Symbol = pair.Symbol,
                    FromDate = fromDate,
                    ToDate = toDate
                });

                var isEmpty = historicalData is null || historicalData.Count() == 0;
                if (!isEmpty) _priceUpdates.Add(pair, new RollingPriceUpdate(pair, historicalData!.Count(), historicalData!));

                if (isEmpty || (!isEmpty && historicalData!.Last().Timestamp.CompareTo(toDate) < 0))
                {
                    if (!isEmpty)
                    {
                        fromDate = historicalData!.Last().Timestamp.AddDays(1);
                    }

                    if (pair.Base.IsFiat)
                    {
                        if (_mappingPairToFeeder.TryGetValue(pair, out var feeder))
                        {
                            var quotes = await feeder.GetHistoricalQuotes(pair.Coin, fromDate, toDate);
                            Task.Run(() => MySQLService.InsertQuotes(quotes));

                            if (!isEmpty) _priceUpdates[pair].AddHistoricalQuotes(quotes.Quotes);
                            else
                            {
                                var newPriceUpdate = new RollingPriceUpdate(pair, quotes.Quotes.Length, quotes.Quotes);
                                _priceUpdates.Add(pair, newPriceUpdate);
                            }
                        }
                        else throw new Exception($"DataFeederService - No feeder for pair {pair.Symbol}.");
                        
                    }
                    else
                    {
                        var pair1 = Config.GetPair($"{pair.Coin}{Pair.SymbolSeparator}USD", pair.Chain);
                        var pair2 = Config.GetPair($"{pair.Base}{Pair.SymbolSeparator}USD", pair.Chain);

                        if (_mappingPairToFeeder.TryGetValue(pair1, out var feeder1) && _mappingPairToFeeder.TryGetValue(pair2, out var feeder2))
                        {
                            var coinQuotes = await feeder1.GetHistoricalQuotes(pair.Coin, fromDate, toDate);
                            var baseQuotes = await feeder2.GetHistoricalQuotes(pair.Coin, fromDate, toDate);

                            var quotes = RecordOperations.Divide(coinQuotes, baseQuotes);
                            Task.Run(() => MySQLService.InsertQuotes(quotes));

                            if (!isEmpty) _priceUpdates[pair].AddHistoricalQuotes(quotes.Quotes);
                            else _priceUpdates.Add(pair, new RollingPriceUpdate(pair, quotes.Quotes.Length, quotes.Quotes));
                        }
                        else throw new Exception($"DataFeederService - One or more feeders missing for pair {pair.Symbol}.");
                    }
                }
            }
        }
    }
}
