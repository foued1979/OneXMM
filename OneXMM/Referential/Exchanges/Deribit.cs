using System;

namespace OneXMM.Referential.Exchanges
{
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges.Requests;

    internal sealed class Deribit : Exchange
    {
        private const string ClientId = "IPxbNR1a";
        private const string Secret = "VaOUj5y2JsBotBNaqpFy7YF3I-_5SfR8_-RVaYJkX8o";
        private readonly string _connectionToken;

        public override bool QuoteInBase => false;
        public override bool IsDiscounted => true;

        public Deribit(bool isTestMode) : base(OneXMM.Exchanges.Deribit, isTestMode)
        {
            if (!isTestMode)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/auth?client_id={ClientId}&client_secret={Secret}&grant_type=client_credentials");
                var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode) throw new Exception($"Cannot connect to Deribit: {response.ReasonPhrase}");
                _connectionToken = Utils.Utils.ConvertJsonToObject<AuthResult>(response.Content.ReadAsStream())!.result.access_token;
            }

            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }
        public override Option[] LoadOptions(string symbol)
        {
            //if (Config.AllOptions.TryGetValue(OneXMM.Exchanges.Deribit, out var deribitOptions) && deribitOptions.IsUpToDate) return deribitOptions.Options;

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/get_instruments?currency={symbol}&expired=false&kind=option");

            var response = _httpClient.SendAsync(request);
            var optionDef = Utils.Utils.ConvertJsonToObject<DeribitGetInstruments>(response.ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStream());
            if (optionDef is null || optionDef.testnet != IsTestMode) throw new Exception("Issue loading options from Deribit.");
            var options =  optionDef.result.Select(o => (Option)o).Where(o => o.OptionType == OptionTypes.Put).ToArray();

            //Config.StoredOptions.UpdateOptions(OneXMM.Exchanges.Deribit, Cutoff, options);

            return options;
        }

        public override Futures[] LoadFutures(string symbol)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/get_instruments?currency={symbol}&expired=false&kind=future");

            var response = _httpClient.SendAsync(request);
            var futureDef = Utils.Utils.ConvertJsonToObject<DeribitGetInstruments>(response.ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStream());
            if (futureDef is null || futureDef.testnet != IsTestMode) throw new Exception("Issue loading futures from Deribit.");
            return futureDef.result.Where(f => !f.instrument_name.Contains("PERPETUAL")).Select(o => (Futures)o).ToArray();
        }

        public override double GetSpot(Pair pair)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/get_order_book?depth=1&instrument_name={pair.ExchangeSymbols[OneXMM.Exchanges.Deribit]}");

            var response = _httpClient.SendAsync(request);
            var spotDef = Utils.Utils.ConvertJsonToObject<DeribitGetOrderBook>(response.ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStream());
            if (spotDef is null) throw new Exception("Issue loading futures from Deribit.");
            return spotDef.result.Spot;
        }

        public override double GetSpot(Futures futures)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/get_order_book_by_instrument_id?depth=1&instrument_id={futures.Id}");

            var response = _httpClient.SendAsync(request);
            var spotDef = Utils.Utils.ConvertJsonToObject<DeribitGetOrderBook>(response.ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStream());
            if (spotDef is null) throw new Exception("Issue loading futures from Deribit.");
            return spotDef.result.Spot;
        }

        public override double GetOptionPrice(Option option)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/get_order_book_by_instrument_id?depth=1&instrument_id={option.Id}");

            var response = _httpClient.SendAsync(request);
            var spotDef = Utils.Utils.ConvertJsonToObject<DeribitGetOrderBook>(response.ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStream());
            if (spotDef is null) throw new Exception("Issue loading options from Deribit.");
            return spotDef.result.Spot;
        }

        public override SortedDictionary<Option, double> GetOptionPrices(Option[] options)
        {
            if (options.Length > 5) throw new ArgumentException($"Deribit - Number of requests: {options.Length}. Max number of requests per second is 5.");

            var responses = new Task[options.Length];
            var prices = new SortedDictionary<Option, double>();

            for (int i = 0; i < options.Length; ++i)
            {
                var index = i;

                responses[index] = Task.Run(() =>
                {
                    var option = options[index];
                    var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/get_order_book_by_instrument_id?depth=1&instrument_id={option.Id}");
                    var response = _httpClient.SendAsync(request);

                    var price = Utils.Utils.ConvertJsonToObject<DeribitGetOrderBook>(response.GetAwaiter().GetResult().Content.ReadAsStream());
                    if (price is null) throw new Exception("Issue loading prices from Deribit.");
                    lock (prices) prices.Add(option, price.result.Spot);
                });
            }
            
            Task.WaitAll(responses);
            return prices;
        }

        public string OptionNameToExpiry(string instrumentName)
        {
            var i1 = instrumentName.IndexOf('-');
            var i2 = instrumentName.IndexOf('-', i1 + 1);
            var expiry = instrumentName.Substring(i1 + 1, i2 - i1 - 1);
            return expiry.Length == 6 ? "0" + expiry : expiry;
        }

        public string FutureNameToExpiry(string instrumentName)
        {
            var i1 = instrumentName.IndexOf('-');
            var expiry = instrumentName.Substring(i1 + 1);
            return expiry.Length == 6 ? "0" + expiry : expiry;
        }

        private sealed class AuthResult
        {
            public Result result { get; set; }

            public sealed class Result
            {
                public string access_token { get; set; }
                public int expires_in { get; set; }
                public string refresh_token { get; set; }
            }
        }

        
    }
}
