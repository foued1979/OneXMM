using System;
using System.Runtime.CompilerServices;

namespace OneXMM
{
    public static class DoubleOperations
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCloseTo(this double v, double from, double tolerance = 1E-6)
        {
            if (System.Math.Abs(from - v) <= tolerance) return true;
            return false;
        }
    }
}
