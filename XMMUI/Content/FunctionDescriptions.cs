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
            { "addApprovedToken", "Adds an approved token to the 1XMM contract.\nApproved tokens are exchangeable against 1XMM." },
            { "addPendingBridgeIn", "Adds a pending bridge to a 1XMM contract." },
            { "addValidChainId", "Adds a blockchain id for which bridge is authorized from the target 1XMM contract." },
            { "balanceOf", "The balance of 1XMM token for a user." },
            { "burn", "Burns a quantity of 1XMM tokens." },
            { "buyBack", "Buys back some 1XMM tokens from a user, in exchange of ETH." },
            { "cancelBridgeIn", "Cancels a bridge in for a user.\nMainly, this should be done if a user cancels the corresponding bridge out request." },
            { "cancelBridgeOut", "Cancels a bridge out.\nMainly, this should be done upon a user's request to cancel." },
            { "getAvailableQuantityOfETH", "Gets the balance of ETH available for change, on a 1XMM contract"},
            { "getAvailableQuantityOfToken", "Gets the balance of a token available for change, on a 1XMM contract." },
            { "getPendingBridgesIn", "Gets the pending bridges for the msg.sender.\n\"Bridge in\" means the 1XMM token position to be created on the queried 1XMM contract" },
            { "getPendingBridgesInForUser", "Gets the pending bridges for a user.\n\"Bridge in\" means the 1XMM token position to be created on the queried 1XMM contract" },
            { "getPendingBridgesOut", "Gets the pending bridges for the msg.sender.\n\"Bridge out\" means the 1XMM token position to be tken out of the queried 1XMM contract" },
            { "getPendingBridgesOutForUser", "Gets the pending bridges for a user.\n\"Bridge out\" means the 1XMM token position to be tken out of the queried 1XMM contract" },
            { "massAllocation", "Allocates tokens to a list of wallets." },
            { "ownerTransfersBack", "Transfers a quantity of 1XMM tokens back to the contract." },
            { "removeETHDepositAuthorization", "Stops possibility to deposit ETH against 1XMM tokens." },
            { "removeETHExchangeAuthorization", "Removes the possibility to exchange 1XMM tokens against ETH." },
            { "removeExchangeAuthorizationForToken", "Removes the possibility to exchange 1XMM tokens a specific token." },
            { "setETHDepositExchangeRate", "Sets the exchange rate when users deposit ETH against 1XMM.\nThis activates the ETH deposit authorization." },
            { "transfer", "Transfers some 1XMM tokens from msg.sender to another user." },
            { "transferFrom", "Transfers some 1XMM tokens from a user to another user.\nAn allowance is required for the msg.sender." },
            { "updateETHExchangeRate", "Updates the ETH exchange rate against 1XMM token.\nThis activates the ETH exchange authorization." },
            { "updateTokenExchangeRate", "Updates the exchange rate for a given token against 1XMM token.\nThis activates the token exchange authorization." },
            { "validateBridgeIn", "Validates a bridge in.\nUser is credited the 1XMM tokens on the destination contract." },
            { "validateBridgeOut", "Validates a bridge out.\nUser's 1XMM tokens are provided back to the AvailableTokens on the origin contract." },
        };
    }
}
