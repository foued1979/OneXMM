using System;

namespace OneXMM.Referential.Exchanges.Responses
{
    internal sealed class DeribitMarkPriceOptions
    {
        public double mark_price { get; set; }
        public string instrument_name { get; set; }
        public double iv { get; set; }
    }
}
