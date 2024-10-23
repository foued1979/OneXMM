using System;

namespace OneXMM.Referential.Exchanges
{
    using OneXMM.Configuration;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;
    using System.Net.WebSockets;

    public abstract class Exchange : IExchange, IDisposable
    {
        private bool _isReadingRequests;
        protected int _reqId;

        protected ExchangeConfig _config;
        protected readonly HttpClient _httpClient;
        protected readonly ClientWebSocket _wsClient;

        public string Name { get; }
        public bool IsTestMode { get; }

        /// <summary>
        /// Gets the cutoff in local time.
        /// </summary>
        public TimeSpan Cutoff { get; }
        public abstract bool QuoteInBase { get; }
        public abstract bool IsDiscounted { get; }
        public bool UseUniversalTime { get; private set; }

        public Exchange(OneXMM.Exchanges exchange, bool isTestMode) 
        {
            Name = exchange.ToString();
            IsTestMode = isTestMode;

            _config = OneXMM.Config.GetExchangeConfig(exchange);
            UseUniversalTime = _config.UseUniversalTime ?? throw new Exception($"Exchange - UseUniversalTime has not been set for {exchange}.");
            Cutoff = _config.Cutoff.ToTimeSpan();
            

            var rpcUri = (isTestMode ? _config.TestRPCUri : _config.ProdRPCUri).TrimEnd('/');
            var wsUri = (isTestMode ? _config.TestWsUri : _config.ProdWsUri).TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(rpcUri);
            _httpClient.DefaultRequestHeaders.Add("jsonrpc", "2.0");

            _wsClient = new ClientWebSocket();
            _wsClient.ConnectAsync(new Uri(wsUri), default).ConfigureAwait(false).GetAwaiter().GetResult();

            Task.Factory.StartNew(() => Listen());
        }

        public abstract Option[] LoadOptions(Pair pair);

        public abstract Futures[] LoadFutures(Pair pair);

        public abstract double GetSpot(Pair pair);

        public abstract double GetSpot(Futures futures);

        public abstract double GetOptionPrice(Option option);

        public abstract string OptionNameToExpiry(string instrumentName);

        public abstract string FutureNameToExpiry(string instrumentName);

        public double GetInterpolatedForward(DateTime maturity, SortedDictionary<DateTime, Futures> futures)
        {
            var spot = GetSpot(futures.First().Value.Pair);

            if (maturity.CompareTo(futures.First().Key) <= 0)
            {
                var fut = futures.First().Value;
                var futPrice = GetSpot(fut);
                fut.SetYield(DateTime.Now, spot, futPrice);
                return futPrice;
            }
            else if (maturity.CompareTo(futures.Last().Key) >= 0)
            {
                var fut = futures.Last().Value;
                var futPrice = GetSpot(fut);
                fut.SetYield(DateTime.Now, spot, futPrice);
                return futPrice;
            }

            int i = 1;
            var compare = 0.0;

            for (i = 1; i < futures.Count; i++) if ((compare = maturity.CompareTo(futures.ElementAt(i).Key)) <= 0) break;
            if (compare == 0.0)
            {
                var fut = futures.ElementAt(i).Value;
                var futPrice = GetSpot(fut);
                fut.SetYield(DateTime.Now, spot, futPrice);
                return futPrice;
            }

            var getSpotTasks = new Task<double>[3];
            getSpotTasks[0] = Task<double>.Run(() => {
                var fut = futures.ElementAt(i - 1).Value;
                var futPrice = GetSpot(fut);
                fut.SetYield(DateTime.Now, spot, futPrice);
                return futPrice;
            });
            getSpotTasks[1] = Task<double>.Run(() => {
                var fut = futures.ElementAt(i).Value;
                var futPrice = GetSpot(fut);
                fut.SetYield(DateTime.Now, spot, futPrice);
                return futPrice;
            });
            getSpotTasks[2] = Task<double>.Run(() =>
            {
                var prop = (maturity - futures.ElementAt(i - 1).Value.Expiry).TotalDays * Constants.OneOver365;
                var tot = (futures.ElementAt(i).Value.Expiry - futures.ElementAt(i - 1).Value.Expiry).TotalDays * Constants.OneOver365;
                return prop / tot;
            });

            Task.WaitAll(getSpotTasks);


            return getSpotTasks[0].Result + getSpotTasks[2].Result * (getSpotTasks[1].Result - getSpotTasks[0].Result);
        }

        public abstract SortedDictionary<Option, double> GetOptionPrices(Pair pair);

        public DateTime ToExpiry(string expiryStr)
        {
            var date = DateTime.ParseExact(expiryStr, _config.ExpiryFormat, null).Add(Cutoff);

            if (UseUniversalTime)
            {
                var offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                date = date.Add(offset).ToUniversalTime();
            }

            return date;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "", default).ConfigureAwait(false).GetAwaiter().GetResult();
            _wsClient.Dispose();
        }

        protected abstract void Authenticate(bool isTestMode);

        protected abstract Task Listen();
    }
}
