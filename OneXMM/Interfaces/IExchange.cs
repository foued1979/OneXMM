using System;

namespace OneXMM.Interfaces
{
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;

    public interface IExchange
    {
        Option[] LoadOptions(string symbol);

        Futures[] LoadFutures(string symbol);

        double GetSpot(Pair pair);

        double GetSpot(Futures futures);

        public double GetOptionPrice(Option option);

        double GetInterpolatedForward(DateTime maturity, SortedDictionary<DateTime, Futures> futures);

        public SortedDictionary<Option, double> GetOptionPrices(Option[] options);
    }
}
