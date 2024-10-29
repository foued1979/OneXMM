using System;

namespace OneXMM
{
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
        setETHDepositExchangeRate,
        transfer,
        transferFrom,
        updateETHExchangeRate,
        updateTokenExchangeRate,
        validateBridgeIn,
        validateBridgeOut
    }
}
