using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OneXMM.Utils
{
    internal static partial class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TAttr? GetAttributeForEnum<TEnum, TAttr>(this TEnum e)
            where TEnum : Enum
            where TAttr : Attribute
        {
            return (typeof(TEnum).GetField(e.ToString())?.GetCustomAttribute(typeof(TAttr)) as TAttr) ?? default(TAttr);
        }
    }
}
