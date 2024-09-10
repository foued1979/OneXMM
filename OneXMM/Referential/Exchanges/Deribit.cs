using System;
using System.Text;

namespace OneXMM.Referential.Exchanges
{
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges.Requests;
    using OneXMM.Referential.Exchanges.Responses;
    using OneXMM.Utils;

    internal sealed class Deribit : Exchange
    {
        private const string ClientId = "IPxbNR1a";
        private const string Secret = "VaOUj5y2JsBotBNaqpFy7YF3I-_5SfR8_-RVaYJkX8o";
        private string _connectionToken;
        private bool _optionsReceived = false;
        private DeribitMarkPriceOptions[] _options = [];
        
        public override bool QuoteInBase => false;
        public override bool IsDiscounted => true;

        public Deribit(bool isTestMode) : base(OneXMM.Exchanges.Deribit, isTestMode)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public override Option[] LoadOptions(Pair pair)
        {
            //if (Config.AllOptions.TryGetValue(OneXMM.Exchanges.Deribit, out var deribitOptions) && deribitOptions.IsUpToDate) return deribitOptions.Options;

            var symbol = pair.GetSymbol(SymbolTypes.Option, OneXMM.Exchanges.Deribit);
            if (string.IsNullOrEmpty(symbol)) throw new Exception($"Deribit - Option symbol missing for pair {pair.Symbol}.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"/public/get_instruments?currency={symbol}&expired=false&kind=option");

            var response = _httpClient.SendAsync(request);
            var optionDef = Utils.ConvertJsonToObject<DeribitGetInstruments>(response.ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStream());
            if (optionDef is null || optionDef.testnet != IsTestMode) throw new Exception("Issue loading options from Deribit.");
            var options =  optionDef.result.Select(o => (Option)o).Where(o => o.OptionType == OptionTypes.Put).ToArray();

            //Config.StoredOptions.UpdateOptions(OneXMM.Exchanges.Deribit, Cutoff, options);

            return options;
        }

        public override Futures[] LoadFutures(Pair pair)
        {
            var symbol = pair.GetSymbol(SymbolTypes.Future, OneXMM.Exchanges.Deribit);
            if (string.IsNullOrEmpty(symbol)) throw new Exception($"Deribit - Future symbol missing for pair {pair.Symbol}.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/get_instruments?currency={symbol}&expired=false&kind=future");

            var response = _httpClient.SendAsync(request);
            var futureDef = Utils.ConvertJsonToObject<DeribitGetInstruments>(response.ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStream());
            if (futureDef is null || futureDef.testnet != IsTestMode) throw new Exception("Issue loading futures from Deribit.");
            return futureDef.result.Where(f => !f.instrument_name.Contains("PERPETUAL")).Select(o => (Futures)o).ToArray();
        }

        public override double GetSpot(Pair pair)
        {
            var symbol = pair.GetSymbol(SymbolTypes.Spot, OneXMM.Exchanges.Deribit);
            if (string.IsNullOrEmpty(symbol)) throw new Exception($"Deribit - Spot symbol missing for pair {pair.Symbol}.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/get_order_book?depth=1&instrument_name={symbol}");

            var response = _httpClient.SendAsync(request);
            var spotDef = Utils.ConvertJsonToObject<DeribitGetOrderBook>(response.ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStream());
            if (spotDef is null) throw new Exception("Issue loading futures from Deribit.");
            return spotDef.result.Spot;
        }

        public override double GetSpot(Futures futures)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/get_order_book_by_instrument_id?depth=1&instrument_id={futures.Id}");

            var response = _httpClient.SendAsync(request);
            var spotDef = Utils.ConvertJsonToObject<DeribitGetOrderBook>(response.ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStream());
            if (spotDef is null) throw new Exception("Issue loading futures from Deribit.");
            return spotDef.result.Spot;
        }

        public override double GetOptionPrice(Option option)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v2/public/get_order_book_by_instrument_id?depth=1&instrument_id={option.Id}");

            var response = _httpClient.SendAsync(request);
            var spotDef = Utils.ConvertJsonToObject<DeribitGetOrderBook>(response.ConfigureAwait(false).GetAwaiter().GetResult().Content.ReadAsStream());
            if (spotDef is null) throw new Exception("Issue loading options from Deribit.");
            return spotDef.result.Spot;
        }

        public override SortedDictionary<Option, double> GetOptionPrices(Pair pair)
        {
            var res = new SortedDictionary<Option, double>();

            int reqId = _reqId++;
            var cmd = $"{{\"method\":\"public/subscribe\",\"params\":{{\"channels\":[\"markprice.options.{pair.GetSymbol(SymbolTypes.Option, OneXMM.Exchanges.Deribit)}\"]}},\"jsonrpc\":\"2.0\",\"id\":{reqId}}}";
            WebsocketSend(cmd);

            while (!_optionsReceived) Thread.Sleep(25);

            cmd = $"{{\"method\":\"public/unsubscribe\",\"params\":{{\"channels\":[\"markprice.options.{pair.GetSymbol(SymbolTypes.Option, OneXMM.Exchanges.Deribit)}\"]}},\"jsonrpc\":\"2.0\",\"id\":{reqId}}}";
            WebsocketSend(cmd);

            _optionsReceived = false;

            Parallel.ForEach(_options, opt => 
            {
                var nameCmpts = opt.instrument_name.Split('-');
                if (nameCmpts[1][2] < '0' || nameCmpts[1][2] > '9') nameCmpts[1] = "0" + nameCmpts[1];

                var option = new Option(this,
                    pair,
                    nameCmpts[3].Equals('C') ? OptionTypes.Call : OptionTypes.Put,
                    Convert.ToDouble(nameCmpts[2]),
                    ToExpiry(nameCmpts[1]));

                lock (res) res.Add(option, opt.mark_price);
            });

            return res;
        }

        public void WebsocketSend(string cmd)
        {
            _wsClient.SendAsync(cmd.ToArraySegment(), System.Net.WebSockets.WebSocketMessageType.Text, true, default);
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

        protected async Task Listen()
        {
            var recBuf = new Memory<byte>(new byte[2048]);

            while (_wsClient.State == System.Net.WebSockets.WebSocketState.Open)
            {
                var payload = new Memory<byte>(new byte[528288]);
                var payloadSize = 0;
                bool contListen = true;

                do
                {
                    var response = await _wsClient.ReceiveAsync(recBuf, default);
                    var rec = recBuf.Slice(0, response.Count);
                    rec.CopyTo(payload.Slice(payloadSize, rec.Length));
                    payloadSize += rec.Length;
                    contListen = response.EndOfMessage;
                }
                while (contListen);

                var jsonString = Encoding.UTF8.GetString(payload.ToArray());

                if (jsonString.Contains("\"id\""))
                {

                }
                else if (jsonString.Contains("\"subscription\""))
                {
                    
                }
            }
        }

        protected override void Authenticate(bool isTestMode)
        {
            if (!isTestMode)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/public/auth?client_id={ClientId}&client_secret={Secret}&grant_type=client_credentials");
                var response = _httpClient.SendAsync(request).GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode) throw new Exception($"Cannot connect to Deribit: {response.ReasonPhrase}");
                _connectionToken = Utils.ConvertJsonToObject<AuthResult>(response.Content.ReadAsStream())!.result.access_token;

                var reqId = _reqId++;
                _wsClient.SendAsync(RequestFactory.CreateDeribitAuthenticationRequest(reqId, ClientId, Secret).ToArraySegment(), System.Net.WebSockets.WebSocketMessageType.Text, true, default)
                    .GetAwaiter()
                    .GetResult();
            }
        }

        private void ReadSubscriptionData(object? data)
        {
            var rawSubscription = Utils.ConvertJsonToObject<DeribitRawSubscriptionResponse>(data.ToString()!);
            if (rawSubscription is null || rawSubscription.Params is null) return;
            var innerData = Utils.ConvertJsonToObject<DeribitDataWrapper>(rawSubscription.Params.ToString()!);

            if (innerData.channel.StartsWith("markprice"))
            {
                var options = Utils.ConvertJsonToObject<DeribitMarkPriceOptions[]>(innerData.data.ToString());
            }
            else throw new NotImplementedException();
        }
    }
}
