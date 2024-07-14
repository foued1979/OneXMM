using System;

namespace OneXMM.Referential
{
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
    }
}
