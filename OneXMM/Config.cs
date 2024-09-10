using System;

namespace OneXMM
{
    using OneXMM.Configuration;
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges;

    public static class Config
    {
        // The private fields
        private static ExchangeSettings _settings;

        internal readonly static Configuration.Referential _referential;
        internal readonly static SymbolMapping _symbolMapping;
        internal readonly static SortedDictionary<string, Exchange> Exchanges = new SortedDictionary<string, Exchange>();

        static Config()
        {
            _settings = SetSettings();
            _referential = SetRefData();
            _symbolMapping = SetSymbolMapping();
        }

        public static ExchangeConfig GetExchangeConfig(Exchanges exchange) => _settings.Exchanges[exchange];

        public static string GetExchangeSymbol(Exchanges exchange, SymbolTypes symbolType, Pair pair)
        {
            if (_symbolMapping.Mapping.TryGetValue(pair.Symbol, out var symbols)
                && symbols.TryGetValue(symbolType, out var symbolsByExchange)
                && symbolsByExchange.TryGetValue(exchange, out var symbol)) return symbol;

            throw new Exception($"Cannot find {symbolType} for Pair {pair.Symbol} on exchange {exchange}.");
        }
        public static Pair? GetPair(Coin coin, Coin counter)
        {
            if (!(_referential.Coins.Contains(coin) && _referential.Coins.Contains(counter))
                || !_symbolMapping.Mapping.TryGetValue($"{coin.Symbol}/{counter.Symbol}", out var exchangeSymbols)) return null;
            return new Pair(coin, counter, exchangeSymbols);
        }

        private static ExchangeSettings SetSettings()
        {
            var appPath = Environment.CurrentDirectory;
            return Utils.Utils.ConvertFileToObject<ExchangeSettings>(Path.Combine(appPath, "Configuration\\ExchangeSettings.json")) ?? throw new Exception("ReferentialData - Issue reading ExchangeSettings.");
        }

        private static Configuration.Referential SetRefData()
        {
            using (var r = typeof(Config).Assembly.GetManifestResourceStream("OneXMM.Configuration.Referential.json"))
            {
                return Utils.Utils.ConvertJsonToObject<Configuration.Referential>(r) ?? throw new Exception("ReferentialData - Issue reading Referential.");
            }
        }

        private static SymbolMapping SetSymbolMapping()
        {
            var appPath = Environment.CurrentDirectory;
            return Utils.Utils.ConvertFileToObject<SymbolMapping>(Path.Combine(appPath, "Configuration\\SymbolMapping.json")) ?? throw new Exception("ReferentialData - Issue reading SymbolMapping.");
        }
    }
}
