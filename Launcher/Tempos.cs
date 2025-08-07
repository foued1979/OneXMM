using System;
using System.Timers;

namespace Launcher
{
    using OneXMM;
    using OneXMM.Referential.Assets;
    using OneXMM.Services;
    using OneXMM.Utils;
    using System.Threading;

    internal static class Tempos
    {
        #region Private Fields

        private static Dictionary<FixingIntervals, List<Pair>> _pairsByUpdateInterval = new Dictionary<FixingIntervals, List<Pair>>(20);
        private static SortedDictionary<Pair, Dictionary<FixingIntervals, FixingIntervals[]>> _redundantIntervals = new SortedDictionary<Pair, Dictionary<FixingIntervals, FixingIntervals[]>>();
        private static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>(20);
        private static Dictionary<System.Timers.Timer, FixingIntervals> _timersToFixingInterval = new Dictionary<System.Timers.Timer, FixingIntervals>(20);

        #endregion

        public static bool TempoStarted { get; private set; } = false;
        public static List<Pair> PairsInScope = new List<Pair>(100);        
        
        public static void AddPairToTempos(Pair pair, OneXMM.Utils.Tools.IntervalSelection intervalSelection)
        {
            if (AddPairToScope(pair))
            {
                VolatilityServices.AddPair(pair);
                PoolRequestManagerServices.AddPair(pair);

                if (intervalSelection.redundantIntervals.Length != 0)
                {
                    Dictionary<FixingIntervals, FixingIntervals[]>? redundants = null;

                    if (!_redundantIntervals.TryGetValue(pair, out redundants))
                    {
                        redundants = new Dictionary<FixingIntervals, FixingIntervals[]>();
                        _redundantIntervals.Add(pair, redundants);
                    }

                    foreach (var redundantInterval in intervalSelection.redundantIntervals)
                    {
                        if (redundants.ContainsKey(redundantInterval.Key)) redundants[redundantInterval.Key] = redundants[redundantInterval.Key].Append(redundantInterval.Value).ToArray();
                        else redundants.Add(redundantInterval.Key, [redundantInterval.Value]);
                    }
                }

                foreach (var fixingInterval in intervalSelection.activeIntervals)
                {
                    List<Pair>? listOfPairs = null;

                    if (!_pairsByUpdateInterval.TryGetValue(fixingInterval, out listOfPairs))
                    {
                        listOfPairs = new List<Pair>(100);
                        _pairsByUpdateInterval.Add(fixingInterval, listOfPairs);

                        var newTimer = new System.Timers.Timer();
                        newTimer.Elapsed += NewTimer_Elapsed;

                        if (TempoStarted) 
                        { 
                            newTimer.Interval = GetTimerInterval(fixingInterval); 
                            newTimer.Start(); 
                        }

                        _timers.Add(newTimer);
                        _timersToFixingInterval.Add(newTimer, fixingInterval);
                    }

                    listOfPairs.Add(pair);
                }
            }
        }

        public static void LaunchTempos()
        {
            TempoStarted = true;

            for (int i = 0; i < _timersToFixingInterval.Count; ++i)
            {
                var index = i;

                Task.Run(() =>
                {
                    var timer = _timersToFixingInterval.ElementAt(index);
#if DEBUG
                    var d = GetTimerInterval(timer.Value);
                    timer.Key.Interval = d;
#else
                    timer.Key.Interval = GetTimerInterval(timer.Value);
#endif
                    timer.Key.Start();
                });
            }
        }

        private static void NewTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (ExchangeService.ReferentialDate != DateTime.Today) ExchangeService.UpdateOptionAndFuturesReferential(PairsInScope);

            var timer = (sender as System.Timers.Timer)!;

            var fixingInterval = _timersToFixingInterval[timer];
            var pairsToUpdate = _pairsByUpdateInterval[fixingInterval];

            var pairsForCorrelatedVol = new List<Pair>();

            foreach (var pair in pairsToUpdate)
            {
                if (pair.CorrelatedVol)
                {
                    pairsForCorrelatedVol.Add(pair);
                    continue;
                }

                // We check if we should use a bigger fixing interval, if the pair as redundant frequencies
                fixingInterval = CheckRedundancy(fixingInterval, pair);

                var exchange = ExchangeService.GetExchange(pair);
                if (exchange.IsCutoffPast()) ExchangeService.UpdateOptionAndFuturesReferential(pair);

                if (!ExchangeService.GetRefInstrumentsForPair(pair, out var refInstr)) throw new Exception($"Tempos - The pair {pair.Symbol} is not marked as correlated vol but there are no ref instruments.");

                var volSurfaceBuilder = Factory.CreateVolSurfaceBuilder(DateTime.Now.ToUniversalTime(), pair, exchange, refInstr.Item1, refInstr.Item2);
                var volSurface = volSurfaceBuilder.Calibrate(fixingInterval);
                VolatilityServices.UpdateVolatilitySurface(volSurface);
            }

            foreach (var pair in pairsForCorrelatedVol)
            {
                Factory.ComputeCorrelatedVol(pair, fixingInterval);
                throw new NotImplementedException();
            }

            timer.Interval = GetTimerInterval(fixingInterval);
        }

        private static FixingIntervals CheckRedundancy(FixingIntervals fixingInterval, Pair pair)
        {
            var currentInterval = fixingInterval;

            if (_redundantIntervals.TryGetValue(pair, out var redundancies) && redundancies.TryGetValue(fixingInterval, out var redundantIntervals))
            {
                var now = DateTime.Now;

                // Since we set up the timer to throw just before a fixing period (up to 6 seconds before)
                // if ever the number of seconds is bigger than 50s, we add 1 minute to the time
                if (now.Minute > 50) now.AddMinutes(1);

                foreach (var interval in redundantIntervals)
                {
                    switch (interval)
                    {
                        // There cannot be any 1 minute interval
                        case FixingIntervals.TwoMinutes:
                            if (now.Minute % 2 == 0 && (int)currentInterval < (int)interval) currentInterval = interval; 
                            break;
                        case FixingIntervals.ThreeMinutes:
                            if (now.Minute % 3 == 0 && (int)currentInterval < (int)interval) currentInterval = interval;
                            break;
                        case FixingIntervals.FiveMinutes:
                            if (now.Minute % 5 == 0 && (int)currentInterval < (int)interval) currentInterval = interval;
                            break;
                        case FixingIntervals.TenMinutes:
                            if (now.Minute % 10 == 0 && (int)currentInterval < (int)interval) currentInterval = interval;
                            break;
                        case FixingIntervals.TwentyMinutes:
                            if (now.Minute % 20 == 0 && (int)currentInterval < (int)interval) currentInterval = interval;
                            break;
                        case FixingIntervals.ThirtyMinutes:
                            if (now.Minute % 30 == 0 && (int)currentInterval < (int)interval) currentInterval = interval;
                            break;
                        case FixingIntervals.OneHour:
                            if (now.Minute == 0 && (int)currentInterval < (int)interval) currentInterval = interval;
                            break;
                        case FixingIntervals.TwoHours:
                            if (now.Hour % 2 == 0 && (int)currentInterval < (int)interval) currentInterval = interval;
                            break;
                        case FixingIntervals.FourHours:
                            if (now.Hour % 4 == 0 && (int)currentInterval < (int)interval) currentInterval = interval;
                            break;
                    }
                }
            }

            return currentInterval;
        }

        private static int GetTimerInterval(FixingIntervals interval)
        {
            var attr = interval.GetAttributeForEnum<FixingIntervals, OneXMM.Attributes.FixingIntervalAttribute>() ?? throw new Exception($"Tempos - Attribute missing for fixing interval {interval}.");
            var now = DateTime.Now;

            switch (interval)
            {
                case FixingIntervals.OneMinute:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 2000;
                case FixingIntervals.TwoMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(0.5 * now.Minute) * 2, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 3000;
                case FixingIntervals.ThreeMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(now.Minute / 3.0) * 3, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 3000;
                case FixingIntervals.FiveMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(0.2 * now.Minute) * 5, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 5000;
                case FixingIntervals.TenMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(0.1 * now.Minute) * 10, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 5000;
                case FixingIntervals.FifteenMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(now.Minute / 15.0) * 15, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 5000;
                case FixingIntervals.TwentyMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(now.Minute / 15.0) * 15, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 5000;
                case FixingIntervals.ThirtyMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(now.Minute / 30.0) * 30, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 5000;
                case FixingIntervals.OneHour:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 5000;
                case FixingIntervals.TwoHours:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, (int)(0.5 * now.Hour) * 2, 0, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 5000;
                case FixingIntervals.FourHours:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, (int)(0.25 * now.Hour) * 4, 0, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalMilliseconds - 5000;
            }

            throw new NotImplementedException();
        }

        private static bool AddPairToScope(Pair pair)
        {
            if (PairsInScope.Contains(pair)) return false;

            PairsInScope.Add(pair);
            return true;
        }
    }
}
