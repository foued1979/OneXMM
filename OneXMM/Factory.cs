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

        public static VolatilitySurface? GetVolatilitySurface(DateTime asof)
        {
            var surfaceDate = new DateTime(asof.Year, asof.Month, asof.Day, asof.ToUniversalTime().Hour, 0, 0, DateTimeKind.Utc);
            var path = Path.Combine(Environment.CurrentDirectory, $"Configuration\\VolSurface_{surfaceDate.ToString("dd-MM-yyyyTHH_mm_ss")}.json");

            if (File.Exists(path))
            {
                return Utils.Utils.ConvertFileToObject<VolatilitySurface>(path, "dd-MM-yyyyTHH:mm:ss");
            }

            return null;
        }

        public static VolatilitySurfaceBuilder CreateVolSurfaceBuilder(DateTime asof, Pair pair, IExchange exchange, Futures[] futures, Option[] options)
        {
            var newSurfaceBuilder = new VolatilitySurfaceBuilder(asof, pair, exchange);

            var fillTasks = new Task[2];

            fillTasks[0] = Task.Run(() => { foreach (var option in options) if (option.Pair.Equals(pair)) newSurfaceBuilder.AddOption(option); });
            fillTasks[1] = Task.Run(() => { foreach (var future in futures) if (future.Pair.Equals(pair)) newSurfaceBuilder.AddFutures(future); });

            Task.WaitAll(fillTasks);
            return newSurfaceBuilder;
        }
    }
}
