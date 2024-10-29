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

        public static Option ToOption(DeribitInstrumentDef optionDef, Blockchains chain)
        {
            var exchange = ExchangeServices.GetExchange(OneXMM.Exchanges.Deribit);
            return new Option(optionDef.instrument_id,
                exchange,
                optionDef.instrument_name,
                Config.GetPair(optionDef.base_currency, optionDef.counter_currency, chain) ?? throw new Exception($"Unknown pair {optionDef.base_currency}/{optionDef.counter_currency} for option on {exchange.Name}."),
                optionDef.option_type.ToLower().Equals("call") ? OptionTypes.Call : OptionTypes.Put,
                optionDef.strike,
                (exchange as Deribit)!.OptionNameToExpiry(optionDef.instrument_name));
        }

        public static Futures ToFutures(DeribitInstrumentDef futDef, Blockchains chain)
        {
            var exchange = ExchangeServices.GetExchange(OneXMM.Exchanges.Deribit);
            return new Futures(futDef.instrument_id,
                exchange,
                futDef.instrument_name,
                Config.GetPair(futDef.base_currency, futDef.counter_currency, chain) ?? throw new Exception($"Unknown pair {futDef.base_currency}/{futDef.counter_currency} for Futures on {exchange.Name}."),
                (exchange as Deribit)!.FutureNameToExpiry(futDef.instrument_name));
        }
    }
}
