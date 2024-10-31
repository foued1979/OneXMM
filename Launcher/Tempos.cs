using System;
using System.Timers;

namespace Launcher
{
    using OneXMM;
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;
    using System.Threading;

    internal static class Tempos
    {
        #region Private Fields

        private static Dictionary<FixingIntervals, List<Pair>> _pairsByUpdateInterval = new Dictionary<FixingIntervals, List<Pair>>(20);
        private static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>(20);
        private static Dictionary<System.Timers.Timer, FixingIntervals> _timersToFixingInterval = new Dictionary<System.Timers.Timer, FixingIntervals>(20);

        #endregion

        public static DateTime Today;
        public static List<Pair> PairsInScope = new List<Pair>(100);        
        
        public static void AddPairToTempos(Pair pair, FixingIntervals[] fixingIntervals)
        {
            if (AddPairToScope(pair))
            {
                VolatilityServices.AddPair(pair);

                foreach (var fixingInterval in fixingIntervals)
                {
                    List<Pair>? listOfPairs = null;

                    if (!_pairsByUpdateInterval.TryGetValue(fixingInterval, out listOfPairs))
                    {
                        listOfPairs = new List<Pair>(100);
                        _pairsByUpdateInterval.Add(fixingInterval, listOfPairs);

                        var newTimer = new System.Timers.Timer();
                        newTimer.Elapsed += NewTimer_Elapsed;
                        _timers.Add(newTimer);
                        _timersToFixingInterval.Add(newTimer, fixingInterval);
                    }

                    listOfPairs.Add(pair);
                }
            }
        }

        public static void LaunchTempos()
        {
            for (int i = 0; i < _timersToFixingInterval.Count; ++i)
            {
                var index = i;

                Task.Run(() =>
                {
                    var timer = _timersToFixingInterval.ElementAt(index);
                    timer.Key.Interval = GetTimerInterval(timer.Value);
                    timer.Key.Start();
                });
            } 
        }

        private static void NewTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (Today != DateTime.Today) ExchangeServices.UpdateOptionAndFuturesReferential(PairsInScope);

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

                var exchange = ExchangeServices.GetExchange(pair);
                if (exchange.IsCutoffTime()) ExchangeServices.UpdateOptionAndFuturesReferential(pair);

                if (!ExchangeServices.GetRefInstrumentsForPair(pair, out var refInstr)) throw new Exception($"Tempos - The pair {pair.Symbol} is not marked as correlated vol but there are no ref instruments.");

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

        private static int GetTimerInterval(FixingIntervals interval)
        {
            var attr = interval.GetAttributeForEnum<FixingIntervals, OneXMM.Attributes.FixingIntervalAttribute>() ?? throw new Exception($"Tempos - Attribute missing for fixing interval {interval}.");
            var now = DateTime.Now;

            switch (interval)
            {
                case FixingIntervals.OneMinute:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 2;
                case FixingIntervals.TwoMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(0.5 * now.Minute) * 2, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 3;
                case FixingIntervals.ThreeMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(now.Minute / 3.0) * 3, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 3;
                case FixingIntervals.FiveMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(0.2 * now.Minute) * 5, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 5;
                case FixingIntervals.TenMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(0.1 * now.Minute) * 10, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 5;
                case FixingIntervals.FifteenMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(now.Minute / 15.0) * 15, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 5;
                case FixingIntervals.TwentyMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(now.Minute / 15.0) * 15, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 5;
                case FixingIntervals.ThirtyMinutes:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, (int)(now.Minute / 30.0) * 30, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 5;
                case FixingIntervals.OneHour:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 5;
                case FixingIntervals.TwoHours:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, (int)(0.5 * now.Hour) * 2, 0, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 5;
                case FixingIntervals.FourHours:
                    return (int)(new DateTime(now.Year, now.Month, now.Day, (int)(0.25 * now.Hour) * 4, 0, 0).AddSeconds(attr.TimeInSeconds) - DateTime.Now).TotalSeconds - 5;
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
