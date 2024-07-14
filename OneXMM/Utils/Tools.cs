using System;

namespace OneXMM.Utils
{
    internal static class Tools
    {
        public static bool IsAround(this DateTime d, DateTime aroundDate)
        {
            return System.Math.Abs((aroundDate - d).TotalSeconds) < 1;
        }
    }
}
