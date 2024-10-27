using System;

namespace OneXMM
{
    using OneXMM.Communication;
    using OneXMM.Communication.MySQLRequests;
    using OneXMM.Communication.MySQLResults;
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
        internal readonly static Communication.MySQLConnector _mysqlConnector;

        static Config()
        {
            _settings = SetSettings();
            _referential = SetRefData();
            _symbolMapping = SetSymbolMapping();
            _poolUpdateCfg = SetPoolUpdateCfg();
            _mysqlConnector = SetMySQL();
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

        public static void InsertQuote(HistoricalResult quote)
        {
            _mysqlConnector.InsertData(new Communication.MySQLRequests.InsertDataRequest(MySQLTables.historical_prices.ToString(), 
                [ [quote.Symbol, quote.Timestamp.ToString(MySQLConnector.DatetimeFormat), quote.Value.ToString()] ]));
        }

        public static IEnumerable<HistoricalResult> GetHistoricalQuote(Pair pair, DateTime? date = null)
        {
            return _mysqlConnector.Get(new GetHistoricalDataRequest(MySQLTables.historical_prices.ToString()) { Symbol = pair.Symbol, Date = date });
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

        private static MySQLConnector SetMySQL()
        {
            var appPath = Path.Combine(Environment.CurrentDirectory, "Configuration\\MySQLSettings.txt");
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

            return new MySQLConnector(settings["USER"], settings["PASSWORD"], settings["MARKETDATA_DB"], Convert.ToInt32(settings["PORT"]));
        }
    }
}
