using System;

namespace OneXMM.Interfaces
{
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;

    public interface IExchange
    {
        bool QuoteInBase { get; }

        bool IsDiscounted { get; }

        Option[] LoadOptions(Pair pair);

        Futures[] LoadFutures(Pair pair);

        double GetSpot(Pair pair);

        double GetSpot(Futures futures);

        public double GetOptionPrice(Option option);

        double GetInterpolatedForward(DateTime maturity, SortedDictionary<DateTime, Futures> futures);

        public SortedDictionary<Option, double> GetOptionPrices(Pair pair);
    }
}
