using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OneXMM.Utils
{
    public static partial class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttr? GetAttributeForEnum<TEnum, TAttr>(this TEnum e)
            where TEnum : Enum
            where TAttr : Attribute
        {
            return (typeof(TEnum).GetField(e.ToString())?.GetCustomAttribute(typeof(TAttr)) as TAttr) ?? default(TAttr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? ToEnum<T>(this string s) 
            where T : Enum
        {
            var ss = s.ToLower();

            foreach (T e in Enum.GetValues(typeof(T)))
            {
                if (e.ToString().ToLower().Equals(ss)) return e;
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PoolMaturities[] ToPoolMaturity(this string[] terms)
        {
            var res = new PoolMaturities[terms.Length];
            for (int i = 0; i < terms.Length; i++) res[i] = terms[i].ToPoolMaturity();
            return res;
        }

        internal static PoolMaturities ToPoolMaturity(this string term)
        {
            if (term.Equals("1D")) return PoolMaturities.OneDay;
            if (term.Equals("1W")) return PoolMaturities.OneWeek;
            if (term.Equals("2W")) return PoolMaturities.TwoWeeks;
            if (term.Equals("3W")) return PoolMaturities.ThreeWeeks;
            if (term.Equals("1M")) return PoolMaturities.OneMonth;
            if (term.Equals("2M")) return PoolMaturities.TwoMonths;
            if (term.Equals("3M")) return PoolMaturities.ThreeMonths;

            throw new Exception($"Unknown term {term}.");
        }
    }
}
