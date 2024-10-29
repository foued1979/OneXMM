using System;
using System.Runtime.CompilerServices;

namespace OneXMM.Utils
{
    using OneXMM.Interfaces;
    
    public static partial class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAccount GetEthereumAccount(int chainId)
        {
            return new EthereumAccount(chainId);
        }
    }
}
