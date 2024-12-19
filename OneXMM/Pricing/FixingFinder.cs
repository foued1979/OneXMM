using System;
using System.Reflection;

namespace OneXMM.Pricing
{
    using OneXMM.Attributes;
    using OneXMM.Interfaces;
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    public static class FixingFinder
    {
        public record FixingDetails(double Spot, double Strike, double Premium);

        public static FixingDetails? FindFixing(DateTime asof, IExchange exchange, IVolatilitySurface surface, PoolMaturities poolMaturity)
        {
            var attr = poolMaturity.GetAttributeForEnum<PoolMaturities, PoolMaturityAttribute>();

            var expiry = asof.Add(attr.Maturity);
            var spot = exchange.GetSpot(surface.Pair);

            var success = MathNet.Numerics.RootFinding.Brent.TryFindRoot(GetFunction(asof, spot, exchange, surface.Pair, expiry, surface), spot, 2 * spot, 1E-4, 250, out double strike);

            if (success)
            {
                var option = new Option(exchange, surface.Pair, OptionTypes.Put, strike, expiry);
                var price = option.Price(asof, spot, surface);
                if (!exchange.QuoteInBase) price *= spot;

                return new FixingDetails(spot, strike, price / strike);
            }
            
            return null;
        }

        private static Func<double, double> GetFunction(DateTime asof, double spot, IExchange exchange, Pair pair, DateTime expiry, IVolatilitySurface volatilitySurface)
        {
            throw new Exception("Option favored by the forward yield should be calibrated such as K_opt - S + Opt = 0 => then we look for strike of other option such as Call = Put");
            return new Func<double, double>((k) =>
            {
                var option = new Option(exchange, pair, OptionTypes.Put, k, expiry);
                var price = option.Price(asof, spot, volatilitySurface);
                if (!exchange.QuoteInBase) price *= spot;
                return k - spot - price;
            });
        }
    }
}
