using System;

namespace OneXMM.Referential.Exchanges.Instruments
{
    internal sealed class DeribitOptionDef
    {
        public string base_currency { get; set; }
        public string counter_currency { get; set; }
        public string quote_currency { get; set; }
        public string option_type { get; set; }
        public string settlement_period { get; set; }
        public double strike { get; set; }
        public string instrument_name { get; set; }

        public DeribitOptionDef() { }

        public static explicit operator Option(DeribitOptionDef optionDef)
        {
            var exchange = Config.Exchanges[OneXMM.Exchanges.Deribit.ToString()];
            return new Option(exchange,
                optionDef.instrument_name,
                Config.RefData.GetPair(optionDef.base_currency, optionDef.counter_currency) ?? throw new Exception($"Unknown pair {optionDef.base_currency}/{optionDef.counter_currency} for option on {exchange.Name}."),
                optionDef.option_type.Equals("Call") ? OptionTypes.Call : OptionTypes.Put,
                optionDef.strike,
                (exchange as Deribit)!.InstrumentNameToExpiry(optionDef.instrument_name));
        }
    }
}
