using System;

namespace OneXMM.Services
{
    using OneXMM.Interfaces;
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
        internal readonly static SortedDictionary<Pair, double> _prices;

        public static bool IsInitialized { get; private set; } = false;
        public static Pair[] Pairs => _prices.Keys.ToArray();

        static DataFeederService()
        {
            var allFeederTypes = Enum.GetValues(typeof(DataFeeders));
            _dataFeeders = new Dictionary<DataFeeders, DataFeeder>(allFeederTypes.Length);
            _mappingPairToFeeder = new SortedDictionary<Pair, DataFeeder>();
            _prices = new SortedDictionary<Pair, double>();

            foreach (DataFeeders dataFeeder in allFeederTypes)
            {
                var feeder = AddDataFeeder(dataFeeder);
                feeder.DataMissingForPair += Feeder_DataMissingForPair;
                feeder.QuotesReceived += Feeder_QuotesReceived;

                foreach (var pair in feeder.Quotes)
                {
                    if (!_mappingPairToFeeder.TryAdd(pair, feeder)) throw new Exception($"DataFeederService - Error adding Pair {pair.Id}: pair is already registered.");
                    if (!_prices.TryAdd(pair, 0)) throw new Exception($"DataFeederService - Error adding Pair {pair.Id}.");
                }

                var lastQuotes = feeder.GetLatestQuotes().ConfigureAwait(false).GetAwaiter().GetResult();
                foreach (var quote in lastQuotes) _prices[Config.GetPair(quote.Id)] = quote.Price;
            }

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
            return _prices.Select(kv => new Quote(kv.Key, kv.Value));
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
            if (!_prices.TryGetValue(pair, out var price)) return null;
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
                _prices[quote.Pair] = quote.Price;
            }
        }
    }
}
