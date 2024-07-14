using System;

namespace OneXMM.Attributes
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    internal class BlockchainDetailsAttribute : Attribute
    {
        public string TestUrl { get; set; }
        public string LiveUrl { get; set; }
        public BlockchainTypes BlockchainType { get; set; }

        public BlockchainDetailsAttribute() { }
    }
}
