using System;

namespace OneXMM
{
    using OneXMM.Configuration;
    using OneXMM.Referential.Exchanges;

    public static class Config
    {
        // The private fields
        private static Settings? _settings = null;
        private static Referential.ReferentialData? _refData = null;

        internal static SortedDictionary<string, Exchange> Exchanges = new SortedDictionary<string, Exchange>();

        private static StoredOptions? _storedOptions = null;

        internal static StoredOptions StoredOptions
        {
            get 
            {
                if (_storedOptions is null) { LoadStoredOptions(); } return _storedOptions
                    ?? throw new Exception("Issue loading stored options."); 
            }
        }

        internal static Dictionary<Exchanges, OptionDetails> AllOptions => StoredOptions!.AllOptions;

        private static StoredFutures? _storedFutures = null;

        internal static StoredFutures StoredFutures
        {
            get 
            { 
                if (_storedFutures is null) LoadStoredFutures(); return _storedFutures
                    ?? throw new Exception("Issue loading stored options.");
            }
        }

        internal static Dictionary<Exchanges, FuturesDetails> AllFutures => StoredFutures!.AllFutures;

        // The public properties
        public static Settings Settings 
        { 
            get { if (_settings is null) SetSettings(); return _settings!; }
        }

        public static Referential.ReferentialData RefData
        {
            get { if (_refData is null) SetRefData(); return _refData; }
        }

        public static ExchangeConfig GetExchangeConfig(Exchanges exchange) => Settings.Exchanges[exchange.ToString()];

        private static void SetSettings()
        {
            var appPath = Environment.CurrentDirectory;
            _settings = Utils.Utils.ConvertFileToObject<Settings>(Path.Combine(appPath, "Configuration\\Settings.json"));
        }

        private static void SetRefData()
        {
            using (var r = typeof(Config).Assembly.GetManifestResourceStream("OneXMM.Configuration.Referential.json"))
            {
                _refData = Utils.Utils.ConvertJsonToObject<Referential.ReferentialData>(r);
            }
        }

        private static void LoadStoredOptions()
        {
            var appPath = Environment.CurrentDirectory;
            _storedOptions = Utils.Utils.ConvertFileToObject<StoredOptions>(Path.Combine(appPath, "Configuration\\StoredOptions.json"), "dd-MM-yyyyTHH:mm:ss");
        }

        private static void LoadStoredFutures()
        {
            var appPath = Environment.CurrentDirectory;
            _storedFutures = Utils.Utils.ConvertFileToObject<StoredFutures>(Path.Combine(appPath, "Configuration\\StoredFutures.json"));
        }
    }
}
