using System;
using NUnit.Framework;
using OneXMM.Referential.Exchanges;

namespace OneXMM._UnitTests
{
    [TestFixture]
    internal class DeribitTests
    {
        [Test]
        public void LoadOptionsTest()
        {
            var deribit = new Deribit(true);
            var options = deribit.LoadOptions(Config.GetPair("BTC/USD", Blockchains.Ethereum));
        }
    }
}
