using System;

namespace OneXMM.Referential
{
    using OneXMM.Pricing;
    using OneXMM.Referential.Exchanges;

    public sealed class Option
    {
        public Exchange Exchange { get; }
        public string Symbol { get; }
        public Assets.Pair Pair { get; }
        public OptionTypes OptionType { get; }
        public double Strike { get; }
        public DateTime Expiry { get; }

        public Option(Exchange exchange, string symbol, Assets.Pair pair, OptionTypes optionType, double strike, string expiry)
        {
            Exchange = exchange;
            Symbol = symbol;
            Pair = pair;
            OptionType = optionType;
            Strike = strike;
            Expiry = exchange.ToExpiry(expiry);
        }

        public double Price(DateTime asof, double spot, VolatilitySurface volSurface)
        {
            var expiry = (Expiry - volSurface.SurfaceDate).TotalDays / 365.0;
            var forward = volSurface.GetForward(spot, asof, Expiry);
            var vol = volSurface.GetVol(forward, Strike, Expiry);

            var volSqrtT = vol * System.Math.Sqrt(expiry);
            var d1 = System.Math.Log(forward / Strike) / volSqrtT + 0.5 * volSqrtT;
            var d2 = d1 - volSqrtT;
            var nsd = new MathNet.Numerics.Distributions.Normal(0, 1);

            return OptionType == OptionTypes.Call
                ? forward * nsd.CumulativeDistribution(d1) - Strike * nsd.CumulativeDistribution(d2)
                : Strike * nsd.CumulativeDistribution(1.0 - d2) - forward * nsd.CumulativeDistribution(1.0 - d1);
        }
    }
}
