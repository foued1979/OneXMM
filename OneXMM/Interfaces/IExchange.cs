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

        public double GetOptionPrice(Option option);

        public double[] GetOptionPrices(Option[] options);
    }
}
