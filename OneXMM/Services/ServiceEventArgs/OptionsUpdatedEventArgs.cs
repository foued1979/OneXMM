using System;

namespace OneXMM.Services.ServiceEventArgs
{
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.Exchanges;

    public sealed class OptionsUpdatedEventArgs : EventArgs
    {
        public Pair Pair { get; }
        public Exchange Exchange { get; }

        public OptionsUpdatedEventArgs(Pair pair, Exchange exchange)
        {
            Pair = pair;
            Exchange = exchange;
        }
    }
}
