using System;

namespace OneXMM
{
    using OneXMM.Attributes;

    public enum TokenFunctions
    {
        allocate,
        allowance,
        approve,
        addApprovedToken,
        addPendingBridgeIn,
        addValidChainId,
        balanceOf,
        burn,
        [ContractFunction("rate", Config.RatePrecision)]
        buyBack,
        cancelBridgeIn,
        cancelBridgeOut,
        getAvailableQuantityOfETH,
        getAvailableQuantityOfToken,
        getPendingBridgesIn,
        getPendingBridgesInForUser,
        getPendingBridgesOut,
        getPendingBridgesOutForUser,
        massAllocation,
        ownerTransfersBack,
        removeETHDepositAuthorization,
        removeETHExchangeAuthorization,
        removeExchangeAuthorizationForToken,
        [ContractFunction("exchangeRate", Config.RatePrecision)]
        setETHDepositExchangeRate,
        transfer,
        transferFrom,
        [ContractFunction("exchangeRate", Config.RatePrecision)]
        updateETHExchangeRate,
        [ContractFunction("exchangeRate", Config.RatePrecision)]
        updateTokenExchangeRate,
        validateBridgeIn,
        validateBridgeOut
    }
}
