using System;

namespace OneXMM.Attributes
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, AllowMultiple = false)]
    internal class PoolMaturityAttribute : Attribute
    {
        public TimeSpan Maturity { get; }

        public PoolMaturityAttribute(int nbDays)
        {
            Maturity = new TimeSpan(nbDays, 0, 0, 0);
        }
    }
}
