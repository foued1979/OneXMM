using System;
using System.Timers;

namespace Launcher
{
    using OneXMM;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    internal static class Tempos
    {
        #region Private Fields

        private static Dictionary<FixingIntervals, List<Pair>> _pairsByUpdateInterval = new Dictionary<FixingIntervals, List<Pair>>(20);
        private static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>(20);
        private static Dictionary<System.Timers.Timer, FixingIntervals> _timersToFixingInterval = new Dictionary<System.Timers.Timer, FixingIntervals>(20);

        #endregion

        public static List<Pair> PairsInScope = new List<Pair>(100);
        
        
        
        public static void AddPairToTempos(Pair pair, FixingIntervals[] fixingIntervals)
        {
            if (AddPairToScope(pair))
            {
                foreach (var fixingInterval in fixingIntervals)
                {
                    List<Pair>? listOfPairs = null;

                    if (!_pairsByUpdateInterval.TryGetValue(fixingInterval, out listOfPairs))
                    {
                        listOfPairs = new List<Pair>(100);
                        _pairsByUpdateInterval.Add(fixingInterval, listOfPairs);

                        var attr = fixingInterval.GetAttributeForEnum<FixingIntervals, OneXMM.Attributes.FixingIntervalAttribute>() ?? throw new Exception($"Tempos - Attribute missing for fixing interval {fixingInterval}.");

                        var newTimer = new System.Timers.Timer(attr.TimeInSeconds);
                        newTimer.Elapsed += NewTimer_Elapsed;
                        _timers.Add(newTimer);
                        _timersToFixingInterval.Add(newTimer, fixingInterval);
                    }

                    listOfPairs.Add(pair);
                }
            }
        }

        private static void NewTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var fixingInterval = _timersToFixingInterval[(sender as System.Timers.Timer)!];
            var pairsToUpdate = _pairsByUpdateInterval[fixingInterval];

            foreach (var pair in pairsToUpdate)
            {
                var exchange = Config.GetExchange(pair);
                var volSurfaceBuilder = Factory.CreateVolSurfaceBuilder(DateTime.Now.ToUniversalTime(), pair, exchange, futures, options);
            }
        }

        private static bool AddPairToScope(Pair pair)
        {
            if (PairsInScope.Contains(pair)) return false;

            PairsInScope.Add(pair);
            return true;
        }
    }
}
