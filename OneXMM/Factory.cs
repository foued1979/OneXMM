using System;

namespace OneXMM
{
    using OneXMM.Interfaces;
    using OneXMM.Pricing;
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;
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

        public static VolatilitySurface CreateVolSurface(DateTime asof, Pair pair, Futures[] futures, Option[] options)
        {
            var newSurface = new VolatilitySurface(pair, asof);

            var fillTasks = new Task[2];

            fillTasks[0] = Task.Run(() => { foreach (var option in options) if (option.Pair.Equals(pair)) newSurface.AddOption(option); });
            fillTasks[1] = Task.Run(() => { foreach (var future in futures) if (future.Pair.Equals(pair)) newSurface.AddFutures(future); });

            Task.WaitAll(fillTasks);
            return newSurface;
        }
    }
}
