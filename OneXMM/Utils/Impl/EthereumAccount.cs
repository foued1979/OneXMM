using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using System;
using System.Numerics;

namespace OneXMM.Utils
{
    using Interfaces;

    public sealed class EthereumAccount : IAccount
    {
        public IWallet Wallet { get; } = new EthereumWallet();
        public Account Account { get; }

        public EthereumAccount(int chainId)
        {
            Account = new Account(Wallet.Signer, new BigInteger(chainId));
        }
    }

    internal sealed class EthereumWallet : IWallet
    {
        // This information should be encrypted
        private const string _privateKey = "0x52aa8ad22e9e2f8b69a16e4af553e8b5274c7ddf01253bbc9a22e355f65f2edb";
        
        public EthECKey Signer { get; }
        public byte[] PublicKey { get; }
        public string WalletId { get; } = "0x1ad724f94190bb5e3185ec7c12374f7a7b7d7c37";

        public EthereumWallet()
        {
            Signer = new EthECKey(_privateKey.HexToByteArray(), true);
            PublicKey = Signer.GetPubKey(false);
        }

        public dynamic Sign(byte[] bytes)
        {
            return Signer.Sign(bytes);
        }
    }
}
