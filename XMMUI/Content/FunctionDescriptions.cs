using System;
using System.Threading;
using System.Windows.Forms;

namespace XMMUI.Content
{
    internal static class FunctionDescriptions
    {
        public static Dictionary<string, string> TokenDescriptions = new Dictionary<string, string>
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

        public static Dictionary<string, string> PoolDescriptions = new Dictionary<string, string>
        {
            { "addIndexofZeros", "Adds the open fixing indices for which a position has reached -100% performance." },
            { "changeCompensationActivation", "Sets whether compensation is activated or not."},
            { "changeCompensationSide", "Changes the Long/Short side to be compensated in case of Loss.\n0: None | 1: Long | 2: Short | 3: Both." },
            { "changeCompensationToken", "Changes the compensation token." },
            { "changeFeeRebateThreshold", "Changes the transaction Fee rebate threshold." },
            { "changeLeverageStatus", "Sets whether leverage is allowed or not." },
            { "changeMinimumDeposit", "Sets the minimum deposit amount on the pool." },
            { "changeMinFeeRebateThreshold", "Sets the minimum fee rebate threshold." },
            { "changeUpdateInterval", "Changes the update interval for the pool.\n0: 1min | 1: 2min | 2: 3min | 3: 5min | 4: 10min | 5: 15min | 6: 20min | 7: 30min | 8: 1h | 9: 3h | 10: 4h" },
            { "closePosition", "Closes the position at given index for msg.sender."},
            { "createPositionForUser", "Closes the position at given index on behalf of user." },
            { "deposit", "Executes a deposit on the pool." },
            { "getAccumulatedFee", "Gets the fee accumulated since inception on the pool." },
            { "getCompensationTokenBalance", "Gets pool's balance of compensation token." },
            { "getFixings", "Gets the fixings recorded on the pool." },
            { "getFixingsFromTo", "Gets the fixings recorded on the pool between 2 dates." },
            { "getAllLastRateIndices", "Gets the last index values for each of the leverage levels." },
            { "getLastRateIndices", "Gets the last index value for a given leverage." },
            { "getMissedFixings", "Gets the list of missed fixing indices." },
            { "getPoolPendingPositions", "Gets the positions which are pending to be accounted at next fixing, for a given leverage level." },
            { "getPoolPositions", "Gets pool alive positions, for a given leverage level." },
            { "getPosition", "Gets the position at given index position, for msg.sender." },
            { "getPositions", "Gets the position of msg.sender."},
            { "reducePosition", "Reduces msg.sender's position at the given index." },
            { "setApplyPenaltyFee", "Sets whether the penalty fee should apply.\nPenalty fee applies when a user closes a position before the expected Term." },
            { "setCompensationRatio", "Sets the compensation ratio.\nCompensation ratio is the percentage of the loss which will be covered by the Compensation Token." },
            { "setConversionRate", "Sets the conversion rate, for the compensation computation.\nConversion rate is the rate: Pool_Token to Compensation_Token." },
            { "setFirstFixing", "Sets the first fixing for the pool.\nSetting the first fixing activates the pool." },
            { "setFixing", "Sets a fixing." },
            { "setNewFee", "Sets the new pool transaction fee for the pool.\nThe fee is expressed in % of basis point, and is computed on a daily basis." },
            { "setNewPenalty", "Sets the new penalty.\nThe Penalty fee applies when a user offsets a position before expected Term." },
            { "updateMissedFixing", "Updates a missed fixing." }
        };
    }
}
