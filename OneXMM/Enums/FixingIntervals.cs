using System;

namespace OneXMM
{
    using OneXMM.Attributes;

    // Fixing Intervals must be sorted, from smaller to bigger interval
    // The sorting is necessary for Tempos::CheckRedundancy
    public enum FixingIntervals
    {
        [FixingInterval(60)]
        OneMinute,
        [FixingInterval(120)]
        TwoMinutes,
        [FixingInterval(180)]
        ThreeMinutes,
        [FixingInterval(300)]
        FiveMinutes,
        [FixingInterval(600)]
        TenMinutes,
        [FixingInterval(900)]
        FifteenMinutes,
        [FixingInterval(1200)]
        TwentyMinutes,
        [FixingInterval(1800)]
        ThirtyMinutes,
        [FixingInterval(3600)]
        OneHour,
        [FixingInterval(7200)]
        TwoHours,
        [FixingInterval(14400)]
        FourHours
    }
}
