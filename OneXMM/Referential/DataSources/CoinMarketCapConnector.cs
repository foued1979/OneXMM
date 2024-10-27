using System;

namespace OneXMM.Referential.DataSources
{
    internal class CoinMarketCapConnector : DataSourceConnector
    {
        private const string ApiKey = "c45d46bd-39b7-4f31-a8b6-634bf2c54c92";

        public CoinMarketCapConnector(string url) : base()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", ApiKey);
            _client.DefaultRequestHeaders.Add("Accepts", "application/json");
            _client.BaseAddress = new Uri(url);
        }
    }
}
