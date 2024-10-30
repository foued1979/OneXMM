using System;

namespace XMMUI.Content
{
    internal static class FunctionDescriptions
    {
        public static Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            { "allocate", "Allocates tokens to a specific wallet." },
            { "allowance", "Checks the allowance of provided to a spender by a wallet owner." },
            { "approve", "Provides an allowance to a spender, on msg.sender's wallet." },
            { "addApprovedToken", "Adds an approved token to the 1XMM contract.\nApproved tokens are exchangeable against 1XMM." }
        };
    }
}
