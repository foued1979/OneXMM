using System;

namespace OneXMM.Communication.MySQLResults
{
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges;
    using OneXMM.Pricing;

    public struct OptionResult
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public OptionTypes OptionType { get; set; }
        public double Strike { get; set; }
        public string UndlSymbol { get; set; }
        public DateTime Expiry { get; set; }
        public Exchange Exchange { get; set; }

        public Option ToOption(Pair pair)
        {
            // Sanity check
            if (!UndlSymbol.Equals(pair.Symbol)) throw new Exception($"OptionResult - The option definition does not match the pair {pair.Symbol}.");
            return new Option(Id, Exchange, Symbol, pair, OptionType, Strike, Expiry);
        }
    }
}
