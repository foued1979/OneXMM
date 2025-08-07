using System;
using System.Data;

namespace OneXMM
{
    using OneXMM.Communication;
    using OneXMM.Configuration;
    using OneXMM.Factories;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using static OneXMM.Utils.Utils;

    public static class Config
    {
        internal const bool IsTestMode = true;
        internal static readonly int nKycStatus = Enum.GetValues<KycStatus>().Length;

        // The private fields
        private static ExchangeSettings _settings;

        // The internal fields
        internal readonly static Configuration.Referential _referential;
        internal readonly static SymbolMapping _symbolMapping;
        internal readonly static SortedDictionary<Pair, Dictionary<KycStatus, Dictionary<FixingIntervals, PoolMaturities[]>>> _poolUpdateCfg = new SortedDictionary<Pair, Dictionary<KycStatus, Dictionary<FixingIntervals, PoolMaturities[]>>>();
        internal readonly static SortedDictionary<Pair, Dictionary<KycStatus, Dictionary<PoolMaturities, string>>> _poolContractAddresses = new SortedDictionary<Pair, Dictionary<KycStatus, Dictionary<PoolMaturities, string>>>();

        #region Public Properties

        public const int RatePrecision = 10000;
        public static NodeInfo NodeInfo { get; private set; }
        public static double MaxEpiry => _settings.MaxEpiry;
        public static DataSettings DataSettings { get; private set; }

        #endregion

        static Config()
        {
            ReadNodeInfo();

            _settings = SetSettings();
            _symbolMapping = SetSymbolMapping();
            _referential = SetReferential();
            DataSettings = SetDataSettings();
            
            foreach (var pairDef in _referential.PairDefs)
            {
                var pair = GetPair(pairDef.CoinId, pairDef.BaseId, true);
                Dictionary<KycStatus, Dictionary<FixingIntervals, PoolMaturities[]>>? matuByKycStatus = null;
                Dictionary<KycStatus, Dictionary<PoolMaturities, string>>? addressesByKycStatus = null;

                if (!_poolUpdateCfg.TryGetValue(pair, out matuByKycStatus))
                {
                    matuByKycStatus = new Dictionary<KycStatus, Dictionary<FixingIntervals, PoolMaturities[]>>(nKycStatus);
                    _poolUpdateCfg.Add(pair, matuByKycStatus);
                } 
                
                if (!_poolContractAddresses.TryGetValue(pair, out addressesByKycStatus))
                {
                    addressesByKycStatus = new Dictionary<KycStatus, Dictionary<PoolMaturities, string>>(nKycStatus);
                    _poolContractAddresses.Add(pair, addressesByKycStatus);
                }

                foreach (var poolSettings in pairDef.PoolSettings)
                {
                    Dictionary<FixingIntervals, PoolMaturities[]>? matuByFixingInterval = null;
                    Dictionary<PoolMaturities, string>? addressByMatu = null;

                    if (!matuByKycStatus.TryGetValue(poolSettings.Key, out matuByFixingInterval))
                    {
                        matuByFixingInterval = new Dictionary<FixingIntervals, PoolMaturities[]>(Enum.GetValues<FixingIntervals>().Length);
                        matuByKycStatus.Add(poolSettings.Key, matuByFixingInterval);
                    }

                    if (!addressesByKycStatus.TryGetValue(poolSettings.Key, out addressByMatu))
                    {
                        addressByMatu = new Dictionary<PoolMaturities, string>(Enum.GetValues<PoolMaturities>().Length);
                        addressesByKycStatus.Add(poolSettings.Key, addressByMatu);
                    }

                    foreach (var poolSetting in poolSettings.Value)
                    {
                        PoolMaturities[]? listOfMatu = null;
                        var poolMatu = poolSetting.Term.ToPoolMaturity();

                        if (!matuByFixingInterval.TryGetValue(poolSetting.FixingInterval, out listOfMatu))
                        {
                            listOfMatu = [poolMatu];
                            matuByFixingInterval.Add(poolSetting.FixingInterval, listOfMatu);
                        }
                        else
                        {
                            listOfMatu = listOfMatu.Append(poolMatu).ToArray();
                        }

                        addressByMatu.Add(poolMatu, IsTestMode ? poolSetting.TestAddress : poolSetting.LiveAddress);
                    }

                    Tools.AdjustMatuByFixingInterval(matuByFixingInterval);
                }
            }

            ContractFactory.SetContractFactory(IsTestMode);
        }

        public static ExchangeConfig GetExchangeConfig(Exchanges exchange) => _settings.Exchanges[exchange];
        public static DataFeederConfig GetDataFeederConfig(DataFeeders dataFeeder) => _settings.DataFeeders[dataFeeder];

        public static Exchanges GetReferenceExchangeForPair(Pair pair)
        {
            var pairDef = _referential.GetPairDef(pair);
            return pairDef.RefExchange;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetTokenContractAddress(Blockchains blockchain, bool isTestMode)
        {
            if (isTestMode) return _referential.TokenAddresses.Test[blockchain];
            else return _referential.TokenAddresses.Live[blockchain];
        }

        public static Dictionary<KycStatus, Dictionary<PoolMaturities, string>> GetPoolContractAddresses(Pair pair, bool isTestMode)
        {
            var res = new Dictionary<KycStatus, Dictionary<PoolMaturities, string>>(nKycStatus);

            if (_poolContractAddresses.TryGetValue(pair, out var addressesByKycStatus)) return addressesByKycStatus;

            throw new Exception($"Config - No pool settings for pair {pair.Symbol} on chain {pair.Chain}.");
        }

        /// <summary>
        /// Gets the sorted pool maturities for a Pair.
        /// </summary>
        /// <param name="pair">The <see cref="Pair"/>.</param>
        public static Dictionary<KycStatus, PoolMaturities[]> GetPoolMaturities(Pair pair)
        {
            var res = new Dictionary<KycStatus, PoolMaturities[]>(nKycStatus);

            foreach (var fixingIntervalDetailsByKycStatus in _poolUpdateCfg[pair])
            {
                var maturities = new List<PoolMaturities>(Enum.GetValues<PoolMaturities>().Length);

                foreach (var matuForKycStatus in fixingIntervalDetailsByKycStatus.Value)
                {
                    foreach (var matu in matuForKycStatus.Value)
                    {
                        if (maturities.Contains(matu)) continue;
                        maturities.Add(matu);
                    }
                }

                maturities.Sort(new PoolMaturityComparer());
                res.Add(fixingIntervalDetailsByKycStatus.Key, maturities.ToArray());
            }

            return res;
        }

        /// <summary>
        /// Gets the sorted pool maturities for a Pair and KycStatus.
        /// </summary>
        /// <param name="pair">The <see cref="Pair"/>.</param>
        /// <param name="kycStatus">The <see cref="KycStatus"/>.</param>
        public static PoolMaturities[] GetPoolMaturities(Pair pair, KycStatus kycStatus)
        {
            if (!_poolUpdateCfg[pair].ContainsKey(kycStatus)) return [];
            var maturities = new List<PoolMaturities>(Enum.GetValues<PoolMaturities>().Length);

            foreach (var fixingIntervalDetailsForKycStatus in _poolUpdateCfg[pair][kycStatus])
            {
                foreach (var matu in fixingIntervalDetailsForKycStatus.Value)
                {
                    if (maturities.Contains(matu)) continue;
                    maturities.Add(matu);
                }

                maturities.Sort(new PoolMaturityComparer());
            }

            return maturities.ToArray();
        }

        /// <summary>
        /// Returns the fixing intervals which are used for a pair.
        /// </summary>
        public static Dictionary<KycStatus, Tools.IntervalSelection> GetFixingIntervalsForPair(Pair pair)
        {
            var res = new Dictionary<KycStatus, Tools.IntervalSelection>(nKycStatus);

            foreach (var kycStatus in _poolUpdateCfg[pair])
            {
                res.Add(kycStatus.Key, GetFixingIntervalsForPair(pair, kycStatus.Key));
            }

            return res;
        }

        /// <summary>
        /// Returns the fixing intervals which are used for a pair and KycStatus.
        /// </summary>
        public static Tools.IntervalSelection GetFixingIntervalsForPair(Pair pair, KycStatus kycStatus)
        {
            var fixingIntervals = _poolUpdateCfg[pair][kycStatus].Keys.ToArray();
            if (fixingIntervals.Length == 0) return new Tools.IntervalSelection([], []);
            return Tools.SplitIntervals(fixingIntervals);
        }
        
        /// <summary>
        /// Returns all the terms which have to be updated for a given fixing interval, for a given pair.
        /// </summary>
        public static Dictionary<KycStatus, PoolMaturities[]> GetTermsToUpdate(FixingIntervals updateTempo, Pair pair)
        {
            var res = new Dictionary<KycStatus, PoolMaturities[]>(nKycStatus);

            if (_poolUpdateCfg.TryGetValue(pair, out var termsByKycStatus)) 
            {
                foreach (var kycStatus in termsByKycStatus)
                {
                    res.Add(kycStatus.Key, GetTermsToUpdate(updateTempo, pair, kycStatus.Key));
                }

                return res;
            }
            throw new Exception("Config - Error getting Terms to update.");
        }

        /// <summary>
        /// Returns all the terms which have to be updated for a given fixing interval, for a given pair and KycStatus
        /// </summary>
        public static PoolMaturities[] GetTermsToUpdate(FixingIntervals updateTempo, Pair pair, KycStatus kycStatus)
        {
            if (_poolUpdateCfg.TryGetValue(pair, out var termsByKycStatus)
                && termsByKycStatus.TryGetValue(kycStatus, out var termsByFixingInterval)
                && termsByFixingInterval.TryGetValue(updateTempo, out var maturities)) return maturities;
            throw new Exception("Config - Error getting Terms to update.");
        }

        public static string GetExchangeSymbol(Exchanges exchange, SymbolTypes symbolType, Pair pair)
        {
            if (_symbolMapping.Mapping.TryGetValue(pair.Symbol, out var symbols)
                && symbols.TryGetValue(symbolType, out var symbolsByExchange)
                && symbolsByExchange.TryGetValue(exchange, out var symbol)) return symbol;

            throw new Exception($"Cannot find {symbolType} for Pair {pair.Symbol} on exchange {exchange}.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pair[] GetPairs()
        {
            return _poolUpdateCfg.Keys.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pair[] GetPairs(KycChoices kycChoice)
        {
            if (kycChoice == KycChoices.Both) return _poolUpdateCfg.Keys.ToArray();
            return _poolUpdateCfg.Keys.Where(k => _poolUpdateCfg[k][kycChoice == KycChoices.NonKyc ? KycStatus.NonKYC : KycStatus.KYC].Count() != 0)?.ToArray() ?? [];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pair[] GetPairs(string pairSymbol)
        {
            return _poolUpdateCfg.Keys.Where(p => p.Symbol.Equals(pairSymbol))?.ToArray()
                ?? throw new Exception($"Config - No pair with symbol {pairSymbol}.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pair GetPair(string pairSymbol, Blockchains chain)
        {
            return _poolUpdateCfg.Keys.First(p => p.Symbol.Equals(pairSymbol) && p.Chain == chain)
                ?? throw new Exception($"Config - No pair with symbol {pairSymbol}.");
        }

        public static Pair GetPair(int pairId)
        {
            var pairDef = _referential.PairDefs.First(pd => pd.Id == pairId);
            var coin = _referential.Coins.First(c => c.Id == pairDef.CoinId);
            var counter = _referential.Coins.First(c => c.Id == pairDef.BaseId);

            if (!_symbolMapping.Mapping.TryGetValue($"{coin.InternalSymbol}{Pair.SymbolSeparator}{counter.InternalSymbol}", out var exchangeSymbols)) throw new Exception($"Config - Coins are not defined for pair {coin.InternalSymbol}/{counter.InternalSymbol}.");
            return new Pair(pairDef.Id, coin, counter, pairDef.CorrelatedVol, exchangeSymbols);
        }

        public static Pair GetPair(int coinId, int baseId, bool createPair)
        {
            if (createPair)
            {
                var pairDef = _referential.PairDefs.First(pd => pd.CoinId == coinId && pd.BaseId == baseId);
                var coin = _referential.Coins.First(c => c.Id == coinId);
                var counter = _referential.Coins.First(c => c.Id == baseId);
                if (!_symbolMapping.Mapping.TryGetValue($"{coin.InternalSymbol}{Pair.SymbolSeparator}{counter.InternalSymbol}", out var exchangeSymbols)) throw new Exception($"Config - Coins are not defined for pair {coin.InternalSymbol}/{counter.InternalSymbol}.");

                return new Pair(pairDef.Id, coin, counter, pairDef.CorrelatedVol, exchangeSymbols);
            }

            return _poolUpdateCfg.Keys.Where(p => p.Coin.Id == coinId && p.Base.Id == baseId)?.First()
                ?? throw new Exception($"Config - No pair for coin Id {coinId} and base Id {baseId}.");
        }

        public static Pair GetPair(string coinSymbol, string counterSymbol, Blockchains chain, bool createPair)
        {
            if (createPair)
            {
                var coin = _referential.Coins.FirstOrDefault(c => c.InternalSymbol.Equals(coinSymbol, StringComparison.OrdinalIgnoreCase) && (c.IsFiat || c.Chain == chain));
                var counter = _referential.Coins.FirstOrDefault(c => c.InternalSymbol.Equals(counterSymbol, StringComparison.OrdinalIgnoreCase) && (c.IsFiat || c.Chain == chain));

                var pairDef = _referential.PairDefs.First(pd => pd.CoinId == coin.Id && pd.BaseId == counter.Id);

                if (coin is null || counter is null || !_symbolMapping.Mapping.TryGetValue($"{coin.InternalSymbol}{Pair.SymbolSeparator}{counter.InternalSymbol}", out var exchangeSymbols)) throw new Exception($"Config - Coins are not defined for pair {coinSymbol}/{counterSymbol}.");
                return new Pair(pairDef.Id, coin, counter, pairDef.CorrelatedVol, exchangeSymbols);
            }

            return _poolUpdateCfg.Keys.Where(p => p.Coin.InternalSymbol.Equals(coinSymbol, StringComparison.OrdinalIgnoreCase)
                && p.Base.InternalSymbol.Equals(counterSymbol, StringComparison.OrdinalIgnoreCase)
                && p.Chain == chain)?.First() ?? throw new Exception($"Config - No pair for coin {coinSymbol} and base {counterSymbol}, on chain {chain}.");
        }

        public static Pair GetPair(Coin coin, Coin counter)
        {
            if (coin.Chain != counter.Chain) return null;
            return GetPair(coin.Id, counter.Id, false);
        }

        private static ExchangeSettings SetSettings()
        {
            var appPath = Environment.CurrentDirectory;
            return Utils.Utils.ConvertFileToObject<ExchangeSettings>(Path.Combine(appPath, "Configuration\\ExchangeSettings.json")) ?? throw new Exception("Config - Issue reading ExchangeSettings.");
        }

        private static DataSettings SetDataSettings()
        {
            var appPath = Environment.CurrentDirectory;
            return Utils.Utils.ConvertFileToObject<DataSettings>(Path.Combine(appPath, "Configuration\\DataSettings.json")) ?? throw new Exception("Config - Issue reading DataSettings.");
        }

        private static Configuration.Referential SetReferential()
        {
            using (var r = typeof(Config).Assembly.GetManifestResourceStream("OneXMM.Configuration.Referential.json"))
            {
                return Utils.Utils.ConvertJsonToObject<Configuration.Referential>(r) ?? throw new Exception("Config - Issue reading Referential.");
            }
        }

        private static SymbolMapping SetSymbolMapping()
        {
            var appPath = Environment.CurrentDirectory;
            return Utils.Utils.ConvertFileToObject<SymbolMapping>(Path.Combine(appPath, "Configuration\\SymbolMapping.json")) ?? throw new Exception("ReferentialData - Issue reading SymbolMapping.");
        }

        private static void ReadNodeInfo()
        {
            var appPath = Path.Combine(AppContext.BaseDirectory, "Configuration\\NodeSettings.txt");
            var settings = new Dictionary<string, string>();

            using (var fileReader = new StreamReader(appPath))
            {
                var line = "";

                while (!string.IsNullOrEmpty(line = fileReader.ReadLine()))
                {
                    var cmp = line.Split('=');
                    settings.Add(cmp[0], cmp[1]);
                }
            }

            NodeInfo = new NodeInfo(settings["NORE_TYPE"].ToEnum<Nodes>(), settings["NODE_API_KEY"]);
        }
    }
}
