using System;

namespace OneXMM
{
    public enum PoolFunctions
    {
        addIndexofZeros,
        changeCompensationActivation,
        changeCompensationSide,
        changeCompensationToken,
        changeFeeRebateThreshold,
        changeLeverageStatus,
        changeMinimumDeposit,
        changeMinFeeRebateThreshold,
        changeUpdateInterval,
        closePosition,
        createPositionForUser,
        deposit,
        getAccumulatedFee,
        getCompensationTokenBalance,
        getFixings,
        getFixingsFromTo,
        getAllLastRateIndices,
        getLastRateIndices,
        getMissedFixings,
        getPoolPendingPositions,
        getPoolPositions,
        getPosition,
        getPositions,
        reducePosition,
        setApplyPenaltyFee,
        setCompensationRatio,
        setConversionRate,
        setFirstFixing,
        setFixing,
        setNewFee,
        setNewPenalty,
        updateMissedFixing
    }
}
