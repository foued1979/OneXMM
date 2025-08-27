using System;
using System.Runtime.CompilerServices;

namespace OneXMM.Utils
{
    using Nethereum.RPC.Eth.DTOs;
    using OneXMM.Attributes;
    using OneXMM.Communication;
    using OneXMM.Math.Models.Smiles;
    using OneXMM.Pricing;
    using Org.BouncyCastle.Tls;

    public static class Tools
    {
        // For a Call
        private const double _moneyness25 = -0.674489750196082;
        private const double _minExp = 0.00136986301369863;
        public record IntervalSelection(FixingIntervals[] activeIntervals, KeyValuePair<FixingIntervals, FixingIntervals>[] redundantIntervals);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAround(this DateTime d, DateTime aroundDate)
        {
            return System.Math.Abs((aroundDate - d).TotalSeconds) < 1;
        }

        /// <summary>
        /// FwdToStrike ratio for 100% vol and simple delta of 20% or 90%
        /// </summary>
        /// <param name="expiry">The target expiry.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double FwdToStrikeRatios25(double expiry)
        {
            expiry = System.Math.Max(expiry, _minExp);
            return System.Math.Exp(_moneyness25 * System.Math.Sqrt(expiry));
        }

        public static SABR[] ExtractSmiles(IEnumerable<SABR> allSmiles, DateTime matu)
        {
            var res = new List<SABR>(allSmiles.Count());

            for (int i = 0; i < allSmiles.Count(); ++i)
            {
                var m = allSmiles.ElementAt(i).SmileExpiry;
                var compare = matu.CompareTo(m);

                if (m.CompareTo(matu) <= 0) res.Add(allSmiles.ElementAt(i));
                else break;
            }

            return res.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateSmileDate(this Dictionary<DateTime, SABR> smiles, DateTime newSmileDate)
        {
            foreach (var smile in smiles) smile.Value.UpdateSmileDate(newSmileDate);
        }

        public static Option[] SelectOptions(this SortedDictionary<double, Option> options, double forward, DateTime asof)
        {
            var strikes = options.Keys;
            var res = new List<Option>(strikes.Count);

            for (int i = 0; i < strikes.Count; ++i) 
            {
                var strike = strikes.ElementAt(i);
                var minRatio = FwdToStrikeRatios25((options.ElementAt(i).Value.Expiry - asof).TotalDays * Constants.OneOver365);

                // We check for Puts
                var r = strike / forward;

                if (r >= minRatio && r <= 1.0)
                {
                    res.Add(options.Values.ElementAt(i));
                }

                if (r > 1.0)
                {
                    res.Add(options.Values.ElementAt(i));
                    break;
                }
            }

            return res.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange(this Dictionary<DateTime, SABR> dict, SABR[] smiles)
        {
            foreach (var s in smiles) dict.TryAdd(s.SmileExpiry, s);
        }

        public static IntervalSelection SplitIntervals(FixingIntervals[] intervals)
        {
            var remainingIntervals = new List<FixingIntervals>(intervals.Length) { intervals[0] };
            var listRedundantIntervals = new List<KeyValuePair<FixingIntervals, FixingIntervals>>(intervals.Length - 1);
            var tempos = new List<int>(intervals.Length) { intervals[0].GetAttributeForEnum<FixingIntervals, FixingIntervalAttribute>()!.TimeInSeconds };

            for (int i = 1; i < intervals.Length; ++i)
            {
                var tempo = intervals[i].GetAttributeForEnum<FixingIntervals, FixingIntervalAttribute>()!.TimeInSeconds;
                var isRedundant = false;

                for(int j = 0; j < remainingIntervals.Count; ++j)
                {
                    if (tempo == tempos[j]) { isRedundant = true; listRedundantIntervals.Add(new KeyValuePair<FixingIntervals, FixingIntervals>(remainingIntervals[j], intervals[i])); break; }
                    if (tempo > tempos[j])
                    {
                        var r = tempo % tempos[j];
                        if (r == 0) { isRedundant = true; listRedundantIntervals.Add(new KeyValuePair<FixingIntervals, FixingIntervals>(remainingIntervals[j], intervals[i])); break; }
                    }
                    else
                    {
                        var r = tempos[j] % tempo;
                        if (r == 0) { isRedundant = true; listRedundantIntervals.Add(new KeyValuePair<FixingIntervals, FixingIntervals>(remainingIntervals[j], intervals[i])); break; }
                    }
                }

                if (isRedundant) continue;

                remainingIntervals.Add(intervals[i]);
                tempos.Add(tempo);
            }

            return new IntervalSelection(remainingIntervals.ToArray(), listRedundantIntervals.ToArray());
        }

        public static SortedDictionary<Option, double> FilterByMaturity(this SortedDictionary<Option, double> allOptions, DateTime maturity)
        {
            var res = new SortedDictionary<Option, double>();
            foreach(var d in allOptions) if (d.Key.Expiry.Equals(maturity)) res.Add(d.Key, d.Value);
            return res;
        }

        public static SortedDictionary<Option, double> FilterByStrike(this SortedDictionary<Option, double> allOptions, Option[] options)
        {
            var res = new SortedDictionary<Option, double>();
            var optionsCopy = (Option[])options.Clone();

            foreach (var option in allOptions) 
                for (int i = 0; i < optionsCopy.Length; i++)
                    if (optionsCopy[i].Equals(option.Key))
                    {
                        res.Add(option.Key, option.Value);
                        optionsCopy.RemoveElementAt(i);
                        break;
                    }

            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] ToInsertData(this TransactionReceipt tx)
        {
            return [DateTime.Now.ToString(MySQLConnector.DatetimeFormat),
                tx.TransactionHash,
                tx.TransactionIndex.ToString(),
                tx.BlockHash,
                tx.BlockNumber.ToString(),
                tx.From,
                tx.To,
                tx.CumulativeGasUsed.Value.ToString(),
                tx.GasUsed.Value.ToString(),
                tx.EffectiveGasPrice.Value.ToString(),
                tx.ContractAddress
            ];
        }

        /// <summary>
        /// Adds terms of shorter fixing intervals to longer fixing intervals.
        /// E.g. if we have 1D updated every 5 minutes, and 1W updated every 10 minutes
        /// We add 1D to the 1W fixing interval so that, when 10 minutes ellapses, we know
        /// that we have to update 1W and 1D.
        /// </summary>
        public static void AdjustMatuByFixingInterval(Dictionary<FixingIntervals, PoolMaturities[]> matuByFixingInterval)
        {
            foreach (FixingIntervals fixingInterval in Enum.GetValues(typeof(FixingIntervals)))
            {
                if (matuByFixingInterval.TryGetValue(fixingInterval, out var maturitiesForInterval))
                {
                    foreach (var otherFixingInterval in matuByFixingInterval.Keys)
                    {
                        if (otherFixingInterval.CompareTo(fixingInterval) > 0)
                        {
                            foreach (var matu in maturitiesForInterval)
                            {
                                if (matuByFixingInterval[otherFixingInterval].Contains(matu)) continue;
                                matuByFixingInterval[otherFixingInterval] = matuByFixingInterval[otherFixingInterval].Append(matu).ToArray();
                            }
                                
                        }
                    }
                }
            }
        }

        public static Tools.IntervalSelection Merge(this Dictionary<KycStatus, Tools.IntervalSelection> dict)
        {
            IEnumerable<FixingIntervals> activeIntervals = new List<FixingIntervals>(Enum.GetValues<FixingIntervals>().Length);
            IEnumerable<KeyValuePair<FixingIntervals, FixingIntervals>> redundantIntervals = new List<KeyValuePair<FixingIntervals, FixingIntervals>>(Enum.GetValues<FixingIntervals>().Length);

            foreach (var kv in dict)
            {
                activeIntervals = activeIntervals.Union(kv.Value.activeIntervals);
                redundantIntervals = redundantIntervals.Union(kv.Value.redundantIntervals);
            }

            return new IntervalSelection(activeIntervals.ToArray(), redundantIntervals.ToArray());
        }

        public static T[] RemoveElementAt<T>(this T[] array, int index)
        {
            if (index >= array.Length) return array;
            array[index] = array.Last();
            return array.SkipLast(1).ToArray();
        }

        public static SortedList<DateTime, IEnumerable<Option>> FilterByExpiry(this IEnumerable<Option> options)
        {
            var res = new SortedList<DateTime, IEnumerable<Option>>();
            foreach (var option in options)
            {
                if (res.ContainsKey(option.Expiry)) res[option.Expiry] = res[option.Expiry].Append(option);
                else res.Add(option.Expiry, new List<Option>() { option });
            }
            return res;
        }

        public static SortedList<TKey, IEnumerable<TValue>> ToSortedList<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>> l)
            where TKey : notnull
        {
            var res = new SortedList<TKey, IEnumerable<TValue>>();

            foreach (var kv in l)
            {
                if (res.TryGetValue(kv.Key, out var r))
                {
                    foreach (var v in kv.Value) res[kv.Key] = r.Append(v);
                }
                else
                {
                    res.Add(kv.Key, kv.Value);
                }
            }

            return res;
        }
    }
}
