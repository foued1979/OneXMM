using System;

namespace OneXMM.Services.ServiceEventArgs
{
    using OneXMM.Referential.Assets;

    public sealed class DataMissingEventArgs : EventArgs
    {
        public Pair Pair { get; }
        public DataFeeders Feeder { get; }

        public DataMissingEventArgs(Pair pair, DataFeeders feeder) 
        { 
            Pair = pair;
            Feeder = feeder;
        }
    }
}
