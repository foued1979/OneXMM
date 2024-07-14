using System;

namespace OneXMM.Referential.Exchanges.Instruments
{
    internal sealed class DeribitOrderBookDef
    {
        public double best_bid_price { get; set; }
        public double best_ask_price { get; set; }
        public double Spot => 0.5 * (best_bid_price + best_ask_price);
    }
}
