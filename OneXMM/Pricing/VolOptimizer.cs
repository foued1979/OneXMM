using System;

namespace OneXMM.Pricing
{
    using OneXMM.Referential.Exchanges;

    internal class VolOptimizer
    {
        private readonly Exchange _exchange;
        private readonly VolatilitySurface _volatilitySurface;

        public VolOptimizer(Exchange exchange, VolatilitySurface volatilitySurface) 
        {
            _exchange = exchange;
            _volatilitySurface = volatilitySurface;
        }

        public void CalibrateSurface()
        {
            var spot = _exchange.GetSpot(_volatilitySurface.Pair);
        }
    }
}
