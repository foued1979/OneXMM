using System;

namespace OneXMM.Referential.Exchanges
{
    using OneXMM.Configuration;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    public abstract class Exchange : IExchange
    {
        private bool _isReadingRequests;

        protected ExchangeConfig _config;
        protected readonly HttpClient _httpClient;

        public string Name { get; }
        public bool IsTestMode { get; }
        public TimeSpan Cutoff { get; }

        public Exchange(OneXMM.Exchanges exchange, bool isTestMode) 
        {
            Name = exchange.ToString();
            IsTestMode = isTestMode;
            _config = OneXMM.Config.GetExchangeConfig(exchange);
            Cutoff = _config.Cutoff.ToTimeSpan();
            

            var uri = (isTestMode ? _config.TestUri : _config.ProdUri).TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"https://{uri}");
            _httpClient.DefaultRequestHeaders.Add("jsonrpc", "2.0");
        }

        public abstract Option[] LoadOptions(string symbol);

        public abstract Futures[] LoadFutures(string symbol);

        public abstract double GetSpot(Pair pair);

        public abstract double GetSpot(Futures futures);

        public abstract double GetOptionPrice(Option option);

        public double GetInterpolatedForward(DateTime maturity, SortedDictionary<DateTime, Futures> futures)
        {
            if (maturity.CompareTo(futures.First().Key) <= 0)
            {
                return GetSpot(futures.First().Value);
            }
            else if (maturity.CompareTo(futures.Last().Key) >= 0)
            {
                return GetSpot(futures.Last().Value);
            }

            int i = 1;
            var compare = 0.0;

            for (i = 1; i < futures.Count; i++) if ((compare = maturity.CompareTo(futures.ElementAt(i).Key)) <= 0) break;
            if (compare == 0.0) return GetSpot(futures.ElementAt(i).Value);

            var getSpotTasks = new Task<double>[3];
            getSpotTasks[0] = Task<double>.Run(() => GetSpot(futures.ElementAt(i - 1).Value));
            getSpotTasks[1] = Task<double>.Run(() => GetSpot(futures.ElementAt(i).Value));
            getSpotTasks[2] = Task<double>.Run(() =>
            {
                var prop = (maturity - futures.ElementAt(i - 1).Value.Expiry).TotalDays * Constants.OneOver365;
                var tot = (futures.ElementAt(i).Value.Expiry - futures.ElementAt(i - 1).Value.Expiry).TotalDays * Constants.OneOver365;
                return prop / tot;
            });

            Task.WaitAll(getSpotTasks);

            return getSpotTasks[0].Result + getSpotTasks[2].Result * (getSpotTasks[1].Result - getSpotTasks[0].Result);
        }

        public abstract SortedDictionary<Option, double> GetOptionPrices(Option[] options);

        public DateTime ToExpiry(string expiryStr)
        {
            return DateTime.ParseExact(expiryStr, _config.ExpiryFormat, null).Add(Cutoff).ToLocalTime();
        }
    }
}
