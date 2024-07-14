using System;

namespace OneXMM
{
    using OneXMM.Interfaces;
    using OneXMM.Referential.Exchanges;

    public static class Factory
    {
        public static IExchange CreateExchange(Exchanges exchange, bool isTestMode)
        {
            switch (exchange)
            {
                case Exchanges.Deribit:
                    var deribit = new Deribit(isTestMode);
                    Config.Exchanges.Add(deribit.Name, deribit);
                    return deribit;
                default:
                    throw new NotImplementedException($"Unknown {exchange}.");
            }
        }
    }
}
