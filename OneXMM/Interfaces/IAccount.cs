using Nethereum.Web3.Accounts;
using System;

namespace OneXMM.Interfaces
{
    public interface IAccount
    {
        public IWallet Wallet { get; }
        public Account Account { get; }
    }
}
