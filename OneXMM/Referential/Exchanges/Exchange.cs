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

        public abstract double GetOptionPrice(Option option);

        public abstract double[] GetOptionPrices(Option[] options);

        public DateTime ToExpiry(string expiryStr)
        {
            return DateTime.ParseExact(expiryStr, _config.ExpiryFormat, null).Add(Cutoff).ToLocalTime();
        }
    }
}
