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
    }
}
