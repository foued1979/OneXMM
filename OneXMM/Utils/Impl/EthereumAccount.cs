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
#if DEBUG
        private const string _privateKey = "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
        public string WalletId { get; } = "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266";
#else
        private const string _privateKey = "0x52aa8ad22e9e2f8b69a16e4af553e8b5274c7ddf01253bbc9a22e355f65f2edb";
        public string WalletId { get; } = "0x1ad724f94190bb5e3185ec7c12374f7a7b7d7c37";
#endif

        public EthECKey Signer { get; }
        public byte[] PublicKey { get; }
        

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
