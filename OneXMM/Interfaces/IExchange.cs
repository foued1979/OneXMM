using System;

namespace OneXMM.Interfaces
{
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;

    public interface IExchange
    {
        string Name { get; }

        bool IsQuotedInBase { get; }

        bool IsDiscounted { get; }

        bool ExchangeUsesUniversalTime { get; }

        bool IsCutoffPast();

        DateTime ToExpiry(string expiry);

        DateTime ToExpiry(DateTime expiry);

        Option[] LoadOptions(Pair pair);

        Futures[] LoadFutures(Pair pair);

        double GetSpot(Pair pair);

        double GetSpot(Futures futures);

        Task<double> GetSpotAsync(Futures futures);

        double[] GetSpot(Futures[] futures);

        double GetOptionPrice(Option option);

        Task<double> GetOptionPriceAsync(Option option);

        double GetInterpolatedForward(DateTime maturity, SortedDictionary<DateTime, Futures> futures);

        SortedList<DateTime, Dictionary<Option, double>> GetOptionPrices(SortedList<DateTime, IEnumerable<Option>> options);
    }
}
