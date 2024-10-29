using System;

namespace OneXMM.Utils
{
    public static partial class Utils
    {
        public static TimeSpan ToTimeSpan(this string val)
        {
            if (val.Length != 5 && (val[2] != 'H' && val[2] != 'h' && val[2] != ':')) throw new ArgumentException($"Wrong timespan format: {val}");

            var hour = Convert.ToInt32(val.Substring(0, 2));
            var min = Convert.ToInt32(val.Substring(3, 2));

            return new TimeSpan(hour, min, 0);
        }
    }
}
