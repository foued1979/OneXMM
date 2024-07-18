using System;

namespace OneXMM.Referential.Exchanges.Responses
{
    internal sealed class DeribitInstrumentDef
    {
        public int instrument_id { get; set; }
        public string base_currency { get; set; }
        public string counter_currency { get; set; }
        public string quote_currency { get; set; }
        public string option_type { get; set; }
        public string settlement_period { get; set; }
        public double strike { get; set; }
        public string instrument_name { get; set; }

        public DeribitInstrumentDef() { }

        public static explicit operator Option(DeribitInstrumentDef optionDef)
        {
            var exchange = Config.Exchanges[OneXMM.Exchanges.Deribit.ToString()];
            return new Option(optionDef.instrument_id,
                exchange,
                optionDef.instrument_name,
                Config.RefData.GetPair(optionDef.base_currency, optionDef.counter_currency) ?? throw new Exception($"Unknown pair {optionDef.base_currency}/{optionDef.counter_currency} for option on {exchange.Name}."),
                optionDef.option_type.ToLower().Equals("call") ? OptionTypes.Call : OptionTypes.Put,
                optionDef.strike,
                (exchange as Deribit)!.OptionNameToExpiry(optionDef.instrument_name));
        }

        public static explicit operator Futures(DeribitInstrumentDef optionDef)
        {
            var exchange = Config.Exchanges[OneXMM.Exchanges.Deribit.ToString()];
            return new Futures(optionDef.instrument_id,
                exchange,
                optionDef.instrument_name,
                Config.RefData.GetPair(optionDef.base_currency, optionDef.counter_currency) ?? throw new Exception($"Unknown pair {optionDef.base_currency}/{optionDef.counter_currency} for option on {exchange.Name}."),
                (exchange as Deribit)!.FutureNameToExpiry(optionDef.instrument_name));
        }
    }
}
