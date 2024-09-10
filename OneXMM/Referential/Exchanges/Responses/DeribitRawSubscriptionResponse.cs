using Newtonsoft.Json;
using System;

namespace OneXMM.Referential.Exchanges.Responses
{
    internal sealed class DeribitRawSubscriptionResponse
    {
        public string jsonrpc { get; set; }
        public string method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public object Params { get; set; }
    }
}
