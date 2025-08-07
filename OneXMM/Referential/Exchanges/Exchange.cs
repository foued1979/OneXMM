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
        public bool IsQuotedInBase { get; }
        public bool IsDiscounted { get; }
        public bool ExchangeUsesUniversalTime { get; private set; }

        public Exchange(OneXMM.Exchanges exchange, bool isTestMode) 
        {
            Name = exchange.ToString();
            IsTestMode = isTestMode;

            _config = OneXMM.Config.GetExchangeConfig(exchange);

            ExchangeUsesUniversalTime = _config.UseUniversalTime ?? throw new Exception($"Exchange - UseUniversalTime has not been set for {exchange}.");
            Cutoff = _config.Cutoff.ToTimeSpan();
            Cutoff = Cutoff.Add(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));
            IsDiscounted = _config.OptionsAreDiscounted;
            IsQuotedInBase = _config.OptionsAreQuotedInBase;
            

            var rpcUri = (isTestMode ? _config.TestRPCUri : _config.ProdRPCUri).TrimEnd('/');
            var wsUri = (isTestMode ? _config.TestWsUri : _config.ProdWsUri).TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(rpcUri);
            _httpClient.DefaultRequestHeaders.Add("jsonrpc", "2.0");

            _wsClient = new ClientWebSocket();
            _wsClient.ConnectAsync(new Uri(wsUri), default).ConfigureAwait(false).GetAwaiter().GetResult();

            Task.Factory.StartNew(() => Listen());
        }

        public bool IsCutoffPast()
        {
            var cutoff = DateTime.Today.Add(Cutoff);
            return DateTime.Now.CompareTo(cutoff) >= 0;
        }

        public abstract Option[] LoadOptions(Pair pair);

        public abstract Futures[] LoadFutures(Pair pair);

        public abstract double GetSpot(Pair pair);

        public abstract double GetSpot(Futures futures);

        public abstract Task<double> GetSpotAsync(Futures futures);

        public double[] GetSpot(Futures[] futures)
        {
            var tasks = new Task<double>[futures.Length];
            var index = 0;

            foreach (var fut in futures)
            {
                tasks[index] = Task.Run(async () => await GetSpotAsync(fut));
                index++;
            }

            Task.WaitAll(tasks);
            return tasks.Select(t => t.Result).ToArray();
        }

        public abstract double GetOptionPrice(Option option);

        public abstract Task<double> GetOptionPriceAsync(Option option);

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

        public abstract SortedList<DateTime, Dictionary<Option, double>> GetOptionPrices(SortedList<DateTime, IEnumerable<Option>> options);

        public DateTime ToExpiry(string expiryStr)
        {
            if (expiryStr.Contains("SEP") && !expiryStr.Contains("SEPT")) expiryStr = expiryStr.Replace("SEP", "SEPT");
            var date = DateTime.ParseExact(expiryStr, _config.ExpiryFormat, null, System.Globalization.DateTimeStyles.AssumeUniversal).Add(Cutoff);

            if (ExchangeUsesUniversalTime)
            {
                var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
                date = date.Add(offset).ToUniversalTime();
            }

            return date;
        }

        public DateTime ToExpiry(DateTime expiry)
        {
            var date = new DateTime(expiry.Year, expiry.Month, expiry.Day).Add(Cutoff);

            if (ExchangeUsesUniversalTime)
            {
                var offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
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
