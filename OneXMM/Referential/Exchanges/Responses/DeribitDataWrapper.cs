using System;

namespace OneXMM.Referential.Exchanges.Responses
{
    internal sealed class DeribitDataWrapper
    {
        public string channel { get; set; } = "";
        public object data { get; set; }
    }
}
