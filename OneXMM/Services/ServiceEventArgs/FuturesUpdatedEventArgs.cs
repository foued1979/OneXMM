using System;

namespace OneXMM.Services.ServiceEventArgs
{
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges;

    public sealed class FuturesUpdatedEventArgs : EventArgs
    {
        public Pair Pair { get; }
        public Exchange Exchange { get; }

        public FuturesUpdatedEventArgs(Pair pair, Exchange exchange)
        {
            Pair = pair;
            Exchange = exchange;
        }
    }
}
