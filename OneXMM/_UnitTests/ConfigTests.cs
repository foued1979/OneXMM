using System;
using NUnit.Framework;

namespace OneXMM._UnitTests
{
    [TestFixture]
    internal class ConfigTests
    {
        [Test]
        public void ConfigTest()
        {
            var feeder = Config.GetDataFeederConfig(DataFeeders.CoinMarketCap);
        }
    }
}
