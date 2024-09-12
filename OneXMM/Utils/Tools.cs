using System;
using System.Runtime.CompilerServices;

namespace OneXMM.Utils
{
    using OneXMM.Pricing;
    using OneXMM.Referential;

    internal static class Tools
    {
        // N-1(0.25)
        private const double _moneyness40 = -0.2533471031358000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAround(this DateTime d, DateTime aroundDate)
        {
            return System.Math.Abs((aroundDate - d).TotalSeconds) < 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double SpotToStrikeRatio(double expiry)
        {
            return Math.Exp(_moneyness40 * Math.Sqrt(expiry));
        }

        public static SABR[] ExtractSmiles(IEnumerable<SABR> allSmiles, DateTime matu)
        {
            for (int i = 0; i < allSmiles.Count(); ++i)
            {
                var m = allSmiles.ElementAt(i).SmileExpiry;
                var compare = matu.CompareTo(m);

                if (compare == 0)
                {
                    return [allSmiles.ElementAt(i)];
                }
                else if (compare < 0)
                {
                    if (i == 0)
                    {
                        return [allSmiles.First()];
                    }

                    var res = new SABR[2];
                    res[0] = allSmiles.ElementAt(i - 1);
                    res[1] = allSmiles.ElementAt(i);
                    return res;
                }
            }

            return [allSmiles.Last()];
        }

        public static Option[] SelectOptions(this SortedDictionary<double, Option> options, double forward)
        {
            var strikes = options.Keys;
            var res = new List<Option>(strikes.Count);

            for (int i = 0; i < strikes.Count; ++i) 
            {
                var strike = strikes.ElementAt(i);

                var strikeToFwdRatio = strike / forward - 1;
                if (Math.Abs(strikeToFwdRatio) <= 0.2)
                {
                    res.Add(options.Values.ElementAt(i));
                }

                if (strikeToFwdRatio >= 0.25) break;
            }

            return res.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange(this Dictionary<DateTime, SABR> dict, SABR[] smiles)
        {
            foreach (var s in smiles) dict.TryAdd(s.SmileExpiry, s);
        }
    }
}
