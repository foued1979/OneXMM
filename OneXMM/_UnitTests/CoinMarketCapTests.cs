using System;
using NUnit.Framework;
using OneXMM.Referential.DataFeeds;

namespace OneXMM._UnitTests
{
    [TestFixture]
    internal class CoinMarketCapTests
    {
        [Test]
        public void GetLatestQuotesTest()
        {
            var feeder = new CoinMarketCap();
            var quotes = feeder.GetLatestQuotes().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Test]
        public async Task GetHistoricalQuotesTest()
        {
            var feeder = new CoinMarketCap();
            var quotes = await feeder.GetHistoricalQuotes(Config.GetPair(0).Coin, DateTime.Today.AddDays(-7), DateTime.Today.AddDays(-2));
        }
    }
}
