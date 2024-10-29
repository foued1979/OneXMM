using System;
using System.Data;

namespace OneXMM
{
    using OneXMM.Communication;
    using OneXMM.Communication.MySQLRequests;
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Configuration;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    public static class Config
    {
        internal const bool IsTestMode = true;

        // The private fields
        private static ExchangeSettings _settings;

        // The internal fields
        internal readonly static Configuration.Referential _referential;
        internal readonly static SymbolMapping _symbolMapping;
        internal readonly static SortedDictionary<Pair, Dictionary<FixingIntervals, string[]>> _poolUpdateCfg = new SortedDictionary<Pair, Dictionary<FixingIntervals, string[]>>();

        static Config()
        {
            _settings = SetSettings();
            _symbolMapping = SetSymbolMapping();
            _referential = SetRefData();
            
            foreach (var pairDef in _referential.PairDefs)
            {
                var pair = GetPair(pairDef.CoinId, pairDef.BaseId);
                Dictionary<FixingIntervals, string[]>? matuByFixingInterval = null;

                if (!_poolUpdateCfg.TryGetValue(pair, out matuByFixingInterval))
                {
                    matuByFixingInterval = new Dictionary<FixingIntervals, string[]>();
                    _poolUpdateCfg.Add(pair, matuByFixingInterval);
                }

                foreach (var poolSettings in pairDef.PoolSettings)
                {
                    string[]? listOfMatu = null;

                    if (!matuByFixingInterval.TryGetValue(poolSettings.FixingInterval, out listOfMatu))
                    {
                        listOfMatu = [poolSettings.Term];
                        matuByFixingInterval.Add(poolSettings.FixingInterval, listOfMatu);
                    }
                    else
                    {
                        listOfMatu = listOfMatu.Append(poolSettings.Term).ToArray();
                    }
                }

                Tools.AdjustMatuByFixingInterval(matuByFixingInterval);
            }
        }

        public static ExchangeConfig GetExchangeConfig(Exchanges exchange) => _settings.Exchanges[exchange];

        public static Exchanges GetReferenceExchangeForPair(Pair pair)
        {
            var pairDef = _referential.GetPairDef(pair);
            return pairDef.RefExchange;
        }

        /// <summary>
        /// Returns the fixing intervals which are used for a pair.
        /// </summary>
        public static FixingIntervals[] GetFixingIntervalsForPair(Pair pair)
        {
            var pairDef = _referential.GetPairDef(pair);
            return pairDef.PoolSettings.Select(ps =>  ps.FixingInterval).ToArray();
        }
        
        /// <summary>
        /// Returns all the terms which have to be updated for a given fixing interval, for a given pair.
        /// </summary>
        public static string[] GetTermsToUpdate(FixingIntervals updateTempo, Pair pair)
        {
            if (_poolUpdateCfg.TryGetValue(pair, out var termsByFixingInterval) && termsByFixingInterval.TryGetValue(updateTempo, out var terms)) return terms;
            throw new Exception("Config - Error getting Terms to update.");
        }

        public static string GetExchangeSymbol(Exchanges exchange, SymbolTypes symbolType, Pair pair)
        {
            if (_symbolMapping.Mapping.TryGetValue(pair.Symbol, out var symbols)
                && symbols.TryGetValue(symbolType, out var symbolsByExchange)
                && symbolsByExchange.TryGetValue(exchange, out var symbol)) return symbol;

            throw new Exception($"Cannot find {symbolType} for Pair {pair.Symbol} on exchange {exchange}.");
        }

        public static Pair[] GetPairs()
        {
            return _referential.PairDefs.Select(pd =>
            {
                var coin = _referential.Coins.First(c => c.Id == pd.CoinId);
                var counter = _referential.Coins.First(c => c.Id == pd.BaseId);
                if (!_symbolMapping.Mapping.TryGetValue($"{coin.Symbol}/{counter.Symbol}", out var exchangeSymbols)) throw new Exception($"Config - Coins are not defined for pair {coin.Symbol}/{counter.Symbol}.");

                return new Pair(coin, counter, pd.CorrelatedVol, exchangeSymbols);
            }).ToArray();
        }

        public static Pair[] GetPairs(string pairSymbol)
        {
            return _referential.PairDefs
                .Select(pd =>
                {
                var coin = _referential.Coins.First(c => c.Id == pd.CoinId);
                var counter = _referential.Coins.First(c => c.Id == pd.BaseId);
                if (!_symbolMapping.Mapping.TryGetValue($"{coin.Symbol}{Pair.SymbolSeparator}{counter.Symbol}", out var exchangeSymbols)) throw new Exception($"Config - Coins are not defined for pair {coin.Symbol}/{counter.Symbol}.");

                return new Pair(coin, counter, pd.CorrelatedVol, exchangeSymbols);
                })
                .Where(p => p.Symbol.Equals(pairSymbol))
                .ToArray();
        }

        public static Pair GetPair(int coinId, int baseId)
        {
            var pairDef = _referential.PairDefs.First(pd => pd.CoinId == coinId && pd.BaseId == baseId);
            var coin = _referential.Coins.First(c => c.Id == coinId);
            var counter = _referential.Coins.First(c => c.Id == baseId);
            if (!_symbolMapping.Mapping.TryGetValue($"{coin.Symbol}{Pair.SymbolSeparator}{counter.Symbol}", out var exchangeSymbols)) throw new Exception($"Config - Coins are not defined for pair {coin.Symbol}/{counter.Symbol}.");

            return new Pair(coin, counter, pairDef.CorrelatedVol, exchangeSymbols);
        }

        public static Pair GetPair(string coinSymbol, string counterSymbol, Blockchains chain)
        {
            var coin = _referential.Coins.FirstOrDefault(c => c.Symbol.Equals(coinSymbol, StringComparison.OrdinalIgnoreCase) && (c.IsFiat || c.Chain == chain));
            var counter = _referential.Coins.FirstOrDefault(c => c.Symbol.Equals(counterSymbol, StringComparison.OrdinalIgnoreCase) && (c.IsFiat || c.Chain == chain));

            var pairDef = _referential.PairDefs.First(pd => pd.CoinId == coin.Id && pd.BaseId == counter.Id);

            if (coin is null || counter is null || !_symbolMapping.Mapping.TryGetValue($"{coin.Symbol}{Pair.SymbolSeparator}{counter.Symbol}", out var exchangeSymbols)) throw new Exception($"Config - Coins are not defined for pair {coinSymbol}/{counterSymbol}.");
            return new Pair(coin, counter, pairDef.CorrelatedVol, exchangeSymbols);
        }

        public static Pair GetPair(Coin coin, Coin counter)
        {
            if (coin.Chain != counter.Chain) return null;
            return GetPair(coin.Id, counter.Id);
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
