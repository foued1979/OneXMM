using System;
using MathNet.Numerics.Optimization;

namespace OneXMM.Pricing
{
    using OneXMM.Attributes;
    using OneXMM.Interfaces;
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    public sealed class VolatilitySurfaceBuilder : VolatilitySurface
    {
        private SortedDictionary<string, Futures> _futuresBySymbol = new SortedDictionary<string, Futures>();
        private SortedDictionary<DateTime, Futures> _futures = new SortedDictionary<DateTime, Futures>();
        private SortedDictionary<DateTime, SABR> _smiles = new SortedDictionary<DateTime, SABR>();

        public VolatilitySurfaceBuilder(DateTime surfaceDate, Pair pair, IExchange exchange) : base(surfaceDate, pair, exchange) { }

        public void UpdateFutures(string symbol, DateTime asof, double spot, double marketPrice)
        {
            if (Exchange.ExchangeUsesUniversalTime) asof = asof.ToUniversalTime();

            if (_futuresBySymbol.TryGetValue(symbol, out var futures))
            {
                futures.SetYield(asof, spot, marketPrice);
            }
        }

        public void AddFutures(Futures futures)
        {
            lock (_futures)
            {
                if (!_futures.ContainsKey(futures.Expiry))
                {
                    _futures.Add(futures.Expiry, futures);
                    _futuresBySymbol.Add(futures.Symbol, futures);
                }
            }
        }

        public void AddOption(Option option)
        {
            lock (_smiles)
            {
                if (!_smiles.ContainsKey(option.Expiry))
                {
                    var sabr = new SABR(SurfaceDate, option.Expiry);
                    _smiles.Add(option.Expiry, sabr);
                    return;
                }
            }

            _smiles[option.Expiry].AddOption(option);
        }

        public IVolatilitySurface Calibrate(FixingIntervals updateTempo)
        {
            GetRequiredSmiles(updateTempo, Pair, _smiles, out var smiles);

            // We get futures prices and we compute the Yields
            var futuresTimestampPrev = Exchange.ExchangeUsesUniversalTime ? DateTime.UtcNow : DateTime.Now;
            var futures = GetFutures(smiles);
            var spot = Exchange.GetSpot(Pair);
            var futuresTimestamp = futuresTimestampPrev.AddSeconds(((Exchange.ExchangeUsesUniversalTime ? DateTime.UtcNow: DateTime.Now) - futuresTimestampPrev).TotalSeconds * 0.5);

            // Since we average the timestamp of the Futures, we update surface date and smile dates
            SurfaceDate = futuresTimestamp;
            smiles.UpdateSmileDate(futuresTimestamp);

            UpdateFuturesPrices(spot, futuresTimestamp, futures, Exchange);

            Yields = new SortedDictionary<DateTime, double>();
            foreach (var fut in futures)
            {
                var isNoExp = fut.Value.Symbol.Equals("NoExp");
                Yields.Add(isNoExp ? futuresTimestamp : fut.Key, fut.Value.Yield);
            }

            var SABRToCalibrate = new Dictionary<SABR, Option[]>(smiles.Count);
            var totalOptionNb = 0;

            for (int i = 0; i < smiles.Count; ++i)
            {
                var smile = smiles.Values.ElementAt(i);
                SABRToCalibrate.Add(smile, smile.GetOptions().SelectOptions(GetForward(spot, futuresTimestamp, smile.SmileExpiry), futuresTimestamp));
                totalOptionNb += SABRToCalibrate.Last().Value.Length;
            }

            var calibrationTimestampStart = Exchange.ExchangeUsesUniversalTime ? DateTime.UtcNow : DateTime.Now;
            SortedDictionary<Option, double>? optionPrices = null;

            // We need to have at least 85% of the number of options that have been identified
            do
            {
                optionPrices = Exchange.GetOptionPrices(Pair);
            } while (optionPrices.Keys.Count < 0.85 * totalOptionNb);

            spot = Exchange.GetSpot(Pair);

            var calibrationTimestamp = Exchange.ExchangeUsesUniversalTime ? DateTime.UtcNow : DateTime.Now;

            // We average time for the calibration
            calibrationTimestamp = calibrationTimestampStart.AddSeconds(0.5 * (calibrationTimestamp - calibrationTimestampStart).TotalSeconds);

#if DEBUG
            var calibrations = new MinimizationResult[SABRToCalibrate.Count];
#else
            var calibrationTasks = new Task<MinimizationResult>[SABRToCalibrate.Count];
#endif
            var result = new SABR[SABRToCalibrate.Count];

            for (int i = 0; i < SABRToCalibrate.Count; ++i)
            {
                var index = i;

                var elementsToCalibrate = SABRToCalibrate.ElementAt(index);
                var forward = GetForward(spot, calibrationTimestamp, elementsToCalibrate.Key.SmileExpiry);

#if DEBUG
                var optionsByMaturity = optionPrices.FilterByMaturity(elementsToCalibrate.Key.SmileExpiry);
                var optionsByStrike = optionsByMaturity.FilterByStrike(elementsToCalibrate.Value);
                calibrations[i] = VolOptimizer.CalibrateSmile(spot, forward, elementsToCalibrate.Key, optionsByStrike);
#else
                calibrationTasks[index] = Task<MinimizationResult>.Run(() =>
                {
                    var optionsByMaturity = optionPrices.FilterByMaturity(elementsToCalibrate.Key.SmileExpiry);
                    var optionsByStrike = optionsByMaturity.FilterByStrike(elementsToCalibrate.Value);
                    return VolOptimizer.CalibrateSmile(spot, forward, elementsToCalibrate.Key, optionsByStrike);
                });
#endif
            }

#if DEBUG

#else
            Task.WaitAll(calibrationTasks);
#endif

            for (int i = 0; i < SABRToCalibrate.Count; ++i)
            {
#if DEBUG
                var parameters = calibrations[i].MinimizingPoint;
#else
                var parameters = calibrationTasks[i].Result.MinimizingPoint;
#endif
                result[i] = SABRToCalibrate.ElementAt(i).Key.WithParameters(parameters[0], parameters[1], parameters[2], parameters[3]);
            }

            foreach (var sabr in result)
            {
                Smiles.Add(sabr.SmileExpiry, sabr);
            }

            return this;
        }

        private static void GetRequiredSmiles(FixingIntervals updateTempo, Pair pair, SortedDictionary<DateTime, SABR> allSmiles, out Dictionary<DateTime, SABR> requiredSmiles)
        {
            requiredSmiles = new Dictionary<DateTime, SABR>(allSmiles.Count);

            var poolMaturities = Config._poolUpdateCfg[pair][updateTempo].ToPoolMaturity();
            var today = allSmiles.First().Key.Kind == DateTimeKind.Utc
                ? new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 0, 0, 0, DateTimeKind.Utc)
                : DateTime.Today;
            var maxMaturity = today;

            foreach (var poolMaturity in poolMaturities)
            {
                var attr = poolMaturity.GetAttributeForEnum<PoolMaturities, PoolMaturityAttribute>() ?? throw new Exception("VolatilitySurfaceBuilder - Issue getting PoolMaturityAttribute.");
                if (today.Add(attr.Maturity).CompareTo(maxMaturity) > 0) maxMaturity = today.Add(attr.Maturity);
            }

            requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, maxMaturity));
        }

        private SortedDictionary<DateTime, Futures> GetFutures(Dictionary<DateTime, SABR> smiles)
        {
            var res = new SortedDictionary<DateTime, Futures>();

            foreach (var smile in smiles)
            {
                if (_futures.ContainsKey(smile.Key)) res.TryAdd(smile.Key, _futures[smile.Key]);
                else
                {
                    var index = 0;
                    while (_futures.ElementAt(index).Value.Expiry.CompareTo(smile.Key) < 0) index++;

                    if (index == 0)
                    {
                        res.TryAdd(smile.Value.SmileDate, Futures.GetNoExpiryFutures(Pair, smile.Value.SmileDate));

                        var fut = _futures.ElementAt(0);
                        res.TryAdd(fut.Key, fut.Value);
                    }
                    else
                    {
                        var fut = _futures.ElementAt(index - 1);
                        res.TryAdd(fut.Key, fut.Value);

                        fut = _futures.ElementAt(index);
                        res.TryAdd(fut.Key, fut.Value);
                    }

                    
                }
            }

            return res;
        }

        private static void UpdateFuturesPrices(double spot, DateTime asof, SortedDictionary<DateTime, Futures> futures, IExchange exchange)
        {
            if (asof.Kind != DateTimeKind.Utc && exchange.ExchangeUsesUniversalTime) throw new Exception("VolatilitySurfaceBuilder - Dates are inconsistent.");
            if (futures.Count <= 4)
            {
                var futuresPriceTasks = new List<Task>(4);
                for (int i = 0; i < futures.Count; i++)
                {
                    int index = i;
                    futuresPriceTasks.Add(Task.Run(() =>
                    {
                        var fut = futures.ElementAt(index).Value;

                        // We exclude the no expiry futures
                        if (fut.Exchange is null && fut.Id == -1) return;

                        var price = exchange.GetSpot(fut);
                        fut.SetYield(asof, spot, price);
                    }));
                }

                Task.WaitAll(futuresPriceTasks.ToArray());
                Thread.Sleep(1000);
            }
            else
            {
                var futuresPriceTasks = new List<Task>(4);

                // This is using DateTime.Now for tempo; no need to adjust to UTC
                var endTime = DateTime.Now.AddSeconds(1);

                for (int i = 0; i < 4; i++)
                {
                    int index = i;
                    futuresPriceTasks.Add(Task.Run(() =>
                    {
                        var fut = futures.ElementAt(index).Value;

                        // We exclude the no expiry futures
                        if (fut.Exchange is null && fut.Id == -1) return;

                        var price = exchange.GetSpot(fut);
                        fut.SetYield(asof, spot, price);
                    }));
                }
                Task.WaitAll(futuresPriceTasks.ToArray());
                while (DateTime.Now.CompareTo(endTime) <= 0) ;

                for (int j = 4; j < futures.Count; j += 5)
                {
                    futuresPriceTasks = new List<Task>(5);

                    // This is using DateTime.Now for tempo; no need to adjust to UTC
                    endTime = DateTime.Now.AddSeconds(1);
                    var count = Math.Min(futures.Count - j, 5);

                    for (int i = 0; i < count; i++)
                    {
                        int index = j + i;

                        futuresPriceTasks.Add(Task.Run(() =>
                        {
                            var fut = futures.ElementAt(index).Value;

                            // We exclude the no expiry futures
                            if (fut.Exchange is null && fut.Id == -1) return;

                            var price = exchange.GetSpot(fut);
                            fut.SetYield(asof, spot, price);
                        }));
                    }

                    Task.WaitAll(futuresPriceTasks.ToArray());
                    while (DateTime.Now.CompareTo(endTime) <= 0) ;
                }
            }
        }
    }
}
