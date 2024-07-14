using System;

namespace OneXMM.Interfaces
{
    using OneXMM.Referential;

    public interface IExchange
    {
        Option[] LoadOptions(string symbol);
    }
}
