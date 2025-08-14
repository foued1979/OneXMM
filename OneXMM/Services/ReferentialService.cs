using System;

namespace OneXMM.Services
{
    using OneXMM.Referential.Assets;
    using System.Timers;

    public static class ReferentialService
    {
        #region Events
        public static event EventHandler<Referential.DataFeeds.Quote> QuoteReceived;
        #endregion

        public static Timer _refreshOptions;
        public static Pair[] Pairs => DataFeederService.Pairs;

        static ReferentialService()
        {
            DataFeederService.QuoteReceived += DataFeederService_QuoteReceived;
            DataFeederService.DataMissing += DataFeederService_DataMissing;

            ExchangeService.FuturesUpdated += ExchangeService_FuturesUpdated;
            ExchangeService.OptionsUpdated += ExchangeService_OptionsUpdated;
            ExchangeService.FuturesAndOptionsUpdateFailed += ExchangeService_FuturesAndOptionsUpdateFailed;

            foreach (var feeder in Config.DataSettings.Feeders) DataFeederService.AddDataFeeder(feeder);
            foreach (var exchange in Config.DataSettings.Exchanges) ExchangeService.AddExchange(exchange);

            Initialize();
        }

        public static void Initialize()
        {
            var quotes = DataFeederService.GetLastQuotes();
            ExchangeService.FilterOptions(quotes);
        }

        public static void Launch()
        {
            DataFeederService.Launch();

            _refreshOptions = new Timer(10000);
            _refreshOptions.Enabled = true;
            _refreshOptions.AutoReset = true;
            _refreshOptions.Elapsed += _refreshOptions_Elapsed;
        }

        private static void _refreshOptions_Elapsed(object? sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ExchangeService_FuturesAndOptionsUpdateFailed(object? sender, Referential.Assets.Pair e)
        {
            throw new NotImplementedException();
        }

        private static void ExchangeService_OptionsUpdated(object? sender, ServiceEventArgs.OptionsUpdatedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ExchangeService_FuturesUpdated(object? sender, ServiceEventArgs.FuturesUpdatedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void DataFeederService_QuoteReceived(object? sender, Referential.DataFeeds.Quote e)
        {
            QuoteReceived(null, e);
        }

        private static void DataFeederService_DataMissing(object? sender, ServiceEventArgs.DataMissingEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
