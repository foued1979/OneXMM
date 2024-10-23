using System;

namespace OneXMM
{
    using Nethereum.Signer;
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
        internal readonly static PoolUpdateCfg _poolUpdateCfg;

        static Config()
        {
            _settings = SetSettings();
            _referential = SetRefData();
            _symbolMapping = SetSymbolMapping();
            _poolUpdateCfg = SetPoolUpdateCfg();
        }

        public static ExchangeConfig GetExchangeConfig(Exchanges exchange) => _settings.Exchanges[exchange];

        public static string GetExchangeSymbol(Exchanges exchange, SymbolTypes symbolType, Pair pair)
        {
            if (_symbolMapping.Mapping.TryGetValue(pair.Symbol, out var symbols)
                && symbols.TryGetValue(symbolType, out var symbolsByExchange)
                && symbolsByExchange.TryGetValue(exchange, out var symbol)) return symbol;

            throw new Exception($"Cannot find {symbolType} for Pair {pair.Symbol} on exchange {exchange}.");
        }

        public static Pair? GetPair(string coinSymbol, string counterSymbol, Blockchains chain)
        {
            if (_referential.PairDefs.Any(p => p.Coin.Equals(coinSymbol, StringComparison.OrdinalIgnoreCase) 
                && p.Base.Equals(counterSymbol, StringComparison.OrdinalIgnoreCase)
                && p.Chain == chain))
            {
                var coin = _referential.Coins.FirstOrDefault(c => c.Symbol.Equals(coinSymbol, StringComparison.OrdinalIgnoreCase) && (c.IsFiat || !c.IsFiat && c.Chain == chain));
                var counter = _referential.Coins.FirstOrDefault(c => c.Symbol.Equals(counterSymbol, StringComparison.OrdinalIgnoreCase) && (c.IsFiat || !c.IsFiat && c.Chain == chain));

                if (coin is null || counter is null || !_symbolMapping.Mapping.TryGetValue($"{coin.Symbol}/{counter.Symbol}", out var exchangeSymbols)) throw new Exception($"Config - Coins are not defined for pair {coinSymbol}/{counterSymbol}.");
                return new Pair(coin, counter, exchangeSymbols);
            }

            return null;
        }

        public static Pair? GetPair(Coin coin, Coin counter)
        {
            if (coin.Chain != counter.Chain) return null;
            return GetPair(coin.Symbol, counter.Symbol, coin.Chain);
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

        private static PoolUpdateCfg SetPoolUpdateCfg()
        {
            var appPath = Environment.CurrentDirectory;
            return Utils.Utils.ConvertFileToObject<PoolUpdateCfg>(Path.Combine(appPath, "Configuration\\PoolUpdateCfg.json")) ?? throw new Exception("ReferentialData - Issue reading PoolUpdateCfg.");
        }
    }
}
