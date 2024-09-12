using System;

namespace OneXMM.Referential.Exchanges.Responses
{
    internal sealed class DeribitIndexDef
    {
        public double index_price { get; set; }
        public double estimated_delivery_price { get; set; }

        public DeribitIndexDef() { }
    }
}
