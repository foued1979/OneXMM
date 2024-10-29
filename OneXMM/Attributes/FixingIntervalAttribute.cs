using System;

namespace OneXMM.Attributes
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class FixingIntervalAttribute : Attribute
    {
        public int TimeInSeconds { get; } = -1;

        public FixingIntervalAttribute(int timeInSeconds)
        {
            TimeInSeconds = timeInSeconds;
        }
    }
}
