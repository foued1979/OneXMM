using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace OneXMM
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Exchanges
    {
        None = 0,
        Deribit
    }
}
