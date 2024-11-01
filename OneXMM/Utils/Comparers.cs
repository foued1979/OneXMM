using System;

namespace OneXMM.Utils
{
    using OneXMM.Attributes;

    public static partial class Utils
    {
        public class PoolMaturityComparer : IComparer<PoolMaturities>
        {
            public int Compare(PoolMaturities x, PoolMaturities y)
            {
                var xMatu = x.GetAttributeForEnum<PoolMaturities, PoolMaturityAttribute>()!;
                var yMatu = y.GetAttributeForEnum<PoolMaturities, PoolMaturityAttribute>()!;

                return xMatu.Maturity.CompareTo(yMatu.Maturity);
            }
        }
    }
}
