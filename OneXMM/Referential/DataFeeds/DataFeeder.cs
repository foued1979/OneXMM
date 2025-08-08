using System;

namespace OneXMM.Referential.DataFeeds
{
    using OneXMM.Configuration;
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.DataFeeds.Events;
    using System.Timers;

    public abstract class DataFeeder
    {
        public string Name { get; }
        public Pair[] Quotes { get; private set; } = [];
        public string ReferenceCurrency { get; }
        public bool LiveQuotesInProgress { get; private set; } = false;

        public event EventHandler<QuoteEventArgs> QuotesReceived;
        public event EventHandler<int> DataMissingForPair;

        protected readonly string _rpcUri;
        protected readonly string? _apiKey;

        protected SemaphoreSlim _sync = new SemaphoreSlim(1, 1);
        protected int _frequency;

        public DataFeeders FeederType { get; }
        private Timer _timer;

        public DataFeeder(DataFeeders dataFeeder, DataFeederConfig config)
        {
            FeederType = dataFeeder;

            ReferenceCurrency = config.ReferenceCurrency;
            Name = dataFeeder.ToString();

            _rpcUri = config.RPCUri;
            _apiKey = config.APIKey;
            _frequency = config.Fequency;

            if (!string.IsNullOrEmpty(config.DefaultQuotes))
            {
                var pairIds = config.DefaultQuotes.Split(',');
                var quotes = new List<Pair>(pairIds.Length);

                foreach (var id in pairIds)
                {
                    var pair = Config.GetPair(Convert.ToInt32(id));

                    // We make sure that pair can be queried via DataFeeder
                    if (!pair.Coin.IsFiat && !pair.Coin.DataFeedInfo.ContainsKey(dataFeeder)) continue;
                    if (!pair.Base.IsFiat && !pair.Base.DataFeedInfo.ContainsKey(dataFeeder)) continue;

                    quotes.Add(pair);
                }

                Quotes = quotes.ToArray();
                InitQuotesAndMessages();
            }
        }

        /// <summary>
        /// Adds a quote to the RPC query.
        /// </summary>
        /// <param name="quote">The quote symbol.</param>
        public void AddQuote(Pair pair)
        {
            _sync.Wait();

            var tmp = Quotes.ToList();
            tmp.Add(pair);
            Quotes = tmp.ToArray();
            InitQuotesAndMessages();

            _sync.Release();
        }

        public abstract Task<OneXMM.Referential.DataFeeds.HistoricalQuotes> GetHistoricalQuotes(Coin coin, DateTime from, DateTime to);
        public abstract Task<IEnumerable<RawQuote>> GetLatestQuotes();

        public void GetLiveQuotes()
        {
            _sync.Wait();
            _timer = new Timer(_frequency);
            _timer.Enabled = true;
            _timer.Elapsed += Timer_Elapsed;

            LiveQuotesInProgress = true;
            _timer.Start();
        }

        public void StopLiveQuotes()
        {
            LiveQuotesInProgress = false;

            _timer.Stop();
            _sync.Release();
            _timer.Dispose();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var quotes = GetLatestQuotes().ConfigureAwait(false).GetAwaiter().GetResult();
            var allQuotes = new List<Quote>(Quotes.Length);

            foreach (var pair in Quotes)
            {
                var coinValue = quotes.Where(q => q.Id == pair.Coin.Id).FirstOrDefault();
                var baseValue = quotes.Where(q => q.Id == pair.Base.Id).FirstOrDefault();

                if (coinValue is null && baseValue is null)
                {
                    DataMissingForPair(this, pair.Id);
                    continue;
                }

                if (coinValue is null) allQuotes.Add(new Quote(pair, 1.0 / baseValue!.Price));
                else if (baseValue is null) allQuotes.Add(new Quote(pair, coinValue.Price));
                else allQuotes.Add(new Quote(pair, coinValue.Price / baseValue.Price));
            }

            QuotesReceived.Invoke(this, new QuoteEventArgs(allQuotes.ToArray()));
        }

        protected abstract void InitQuotesAndMessages();
    }
}
