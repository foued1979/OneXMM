using Nethereum.Signer;
using System;

namespace OneXMM.Interfaces
{
    public interface IWallet
    {
        public EthECKey Signer { get; }
        public byte[] PublicKey { get; }
        public string WalletId { get; }

        /// <summary>Signs a transaction.</summary>
        /// <param name="bytes">The <see href="byte[]"/> message to sign.</param>
        public dynamic Sign(byte[] bytes);
    }
}
