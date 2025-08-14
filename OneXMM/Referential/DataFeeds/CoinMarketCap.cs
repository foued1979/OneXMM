using Newtonsoft.Json;
using System;
using System.Text;

namespace OneXMM.Referential.DataFeeds
{
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.DataFeeds.Responses;

    public sealed class CoinMarketCap : DataFeeder
    {
        private const string HistoricalQuotesEndPoint = "quotes/historical";
        private const string LatestQuotesEndPoint = "quotes/latest";

        private string _listOfItems = "";

        private HttpRequestMessage? _latestQuoteMsg = null;

        public CoinMarketCap() : base(DataFeeders.CoinMarketCap, Config.GetDataFeederConfig(DataFeeders.CoinMarketCap))
        { }

        public override async Task<OneXMM.Referential.DataFeeds.HistoricalQuotes> GetHistoricalQuotes(Coin coin, DateTime from, DateTime to)
        {
            if (!coin.DataFeedInfo.ContainsKey(DataFeeders.CoinMarketCap)) return new HistoricalQuotes(coin, Config.GetCoin("USD", null), []);

            var id = coin.DataFeedInfo[DataFeeders.CoinMarketCap].Id;
            var n = (int)(to - from).TotalDays + 1;
            var timeEnd = new DateTime(to.Year, to.Month, to.Day, 23, 59, 59, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var msg = new HttpRequestMessage(HttpMethod.Get, string.Format("{0}{1}?id={2}&time_end={3}&count={4}&interval=24h&convert={5}",
                _rpcUri,
                HistoricalQuotesEndPoint,
                coin.DataFeedInfo[DataFeeders.CoinMarketCap].Id,
                timeEnd,
                n,
                ReferenceCurrency));

            msg.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            msg.Headers.Add("X-CMC_PRO_API_KEY", _apiKey);

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.SendAsync(msg);

                using (var stream = new StreamReader(response.Content.ReadAsStream()))
                using (var reader = new JsonTextReader(stream))
                {
                    var quote = new JsonSerializer().Deserialize<CoinMarketCapHistoricalQuote>(reader);
                    return new HistoricalQuotes(coin, Config.GetCoin("USD", null), quote.data.quotes.Select(q => new HistoricalQuote(q.timestamp, q.quote[ReferenceCurrency].price)).ToArray());
                }
            }
        }

        public override async Task<IEnumerable<RawQuote>> GetLatestQuotes()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.SendAsync(_latestQuoteMsg);

                using (var stream = new StreamReader(response.Content.ReadAsStream()))
                using (var reader = new JsonTextReader(stream))
                {
                    var quote = new JsonSerializer().Deserialize<CoinMarketCapQuote>(reader);
                    return quote.data.Select(d => new RawQuote(d.Key, d.Value.quote[ReferenceCurrency].price));
                }
            }
        }

        protected override void InitQuotesAndMessages()
        {
            var listOfQuotes = new List<int>();
            var sb = new StringBuilder();

            foreach (var quote in Quotes)
            {
                if (!quote.Base.InternalSymbol.Equals(ReferenceCurrency))
                {
                    var id = quote.Coin.DataFeedInfo[DataFeeders.CoinMarketCap].Id;
                    if (!listOfQuotes.Contains(id)) listOfQuotes.Add(id);

                    id = quote.Base.DataFeedInfo[DataFeeders.CoinMarketCap].Id;
                    if (!listOfQuotes.Contains(id)) listOfQuotes.Add(id);
                }
                else
                {
                    var id = quote.Coin.DataFeedInfo[DataFeeders.CoinMarketCap].Id;
                    if (!listOfQuotes.Contains(id)) listOfQuotes.Add(id);
                }
            }
            
            foreach (var quote in listOfQuotes)
            {
                sb.Append(quote.ToString() + ',');
            }
            
            _listOfItems = sb.ToString().TrimEnd(',');

            _latestQuoteMsg = new HttpRequestMessage(HttpMethod.Get, string.Format("{0}{1}?id={2}&convert={3}", _rpcUri, LatestQuotesEndPoint, _listOfItems, ReferenceCurrency));
            _latestQuoteMsg.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _latestQuoteMsg.Headers.Add("X-CMC_PRO_API_KEY", _apiKey);
        }
    }
}
