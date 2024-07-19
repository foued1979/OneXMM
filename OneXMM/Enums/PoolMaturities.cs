using System;

namespace OneXMM
{
    using Attributes;

    public enum PoolMaturities
    {
        [PoolMaturity(1)]
        OneDay,
        [PoolMaturity(7)]
        OneWeek,
        [PoolMaturity(14)]
        TwoWeeks,
        [PoolMaturity(21)]
        ThreeWeeks,
        [PoolMaturity(30)]
        OneMonth,
        [PoolMaturity(60)]
        TwoMonths,
        [PoolMaturity(90)]
        ThreeMonths
    }
}
