using System;

namespace OneXMM.Pricing
{
    using MathNet.Numerics.Optimization;
    using OneXMM.Interfaces;
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;
    using OneXMM.Utils;

    public sealed class VolatilitySurfaceBuilder : VolatilitySurface
    {
        private SortedDictionary<string, Futures> _futuresBySymbol = new SortedDictionary<string, Futures>();
        private SortedDictionary<DateTime, Futures> _futures = new SortedDictionary<DateTime, Futures>();
        private SortedDictionary<DateTime, SABR> _smiles = new SortedDictionary<DateTime, SABR>();

        public IExchange Exchange { get; }

        public VolatilitySurfaceBuilder(DateTime surfaceDate, Pair pair, IExchange exchange) :base(surfaceDate, pair)
        {
            Exchange = exchange;
        }

        public void UpdateFutures(string symbol, DateTime asof, double spot, double marketPrice)
        {
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

        public void Calibrate(VolUpdateTempos updateTempo)
        {
            GetRequiredSmiles(updateTempo, _smiles, out var smiles);

            // We get futures prices and we compute the Yields
            var futures = GetFutures(smiles);
            var spot = Exchange.GetSpot(Pair);
            var futuresTimestamp = DateTime.Now;
            UpdateFuturesPrices(spot, futuresTimestamp, futures, Exchange);

            Task.Run(() =>
            {
                Yields = new SortedDictionary<DateTime, double>();
                foreach (var fut in futures)
                {
                    Yields.Add(fut.Key, fut.Value.Yield);
                }
            });

            var SABRToCalibrate = new Dictionary<SABR, Option[]>(smiles.Count);

            for (int i = 0; i < smiles.Count; ++i)
            {
                var smile = smiles.Values.ElementAt(i);
                SABRToCalibrate.Add(smile, smile.GetOptions().SelectOptions(GetForward(spot, futuresTimestamp, smile.SmileExpiry)));
            }

            var optionPrices = Exchange.GetOptionPrices(Pair);

            //var calibrationTasks = new Task<MinimizationResult>[SABRToCalibrate.Count];
            //var prices = new SortedDictionary<Option, double>[SABRToCalibrate.Count];
            //var result = new SABR[SABRToCalibrate.Count];

            //spot = Exchange.GetSpot(Pair);
            //var calibrationTimestamp = DateTime.Now;
            //Thread.Sleep(1000);

            //for (int i = 0; i < SABRToCalibrate.Count; ++i)
            //{
            //    var index = i;

            //    var elementsToCalibrate = SABRToCalibrate.ElementAt(i).Value;
            //    var forward = GetForward(spot, calibrationTimestamp, SABRToCalibrate.Keys.ElementAt(i).SmileExpiry);

            //    var stopTime = DateTime.Now.AddSeconds(1);
            //    prices[index] = Exchange.GetOptionPrices(elementsToCalibrate);

            //    calibrationTasks[index] = Task<MinimizationResult>.Run(() =>
            //    {
            //        return VolOptimizer.CalibrateSmile(spot, forward, SABRToCalibrate.ElementAt(index).Key, prices[index]);
            //    });

            //    while (DateTime.Now.CompareTo(stopTime) < 0) ;
            //}

            //Task.WaitAll(calibrationTasks);

            //for (int i = 0; i < SABRToCalibrate.Count; ++i)
            //{
            //    var parameters = calibrationTasks[i].Result.MinimizingPoint;
            //    result[i] = SABRToCalibrate.ElementAt(i).Key.WithParameters(parameters[0], parameters[1], parameters[2], parameters[3]);
            //}

            //foreach (var sabr in result)
            //{
            //    Smiles.Add(sabr.SmileExpiry, sabr);
            //}
        }

        private static void GetRequiredSmiles(VolUpdateTempos updateTempo, SortedDictionary<DateTime, SABR> allSmiles, out Dictionary<DateTime, SABR> requiredSmiles)
        {
            requiredSmiles = new Dictionary<DateTime, SABR>(allSmiles.Count);
            var now = DateTime.Now;

            switch (updateTempo)
            {
                case VolUpdateTempos.TenMinutes:
                    var oneDayMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(1);
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneDayMaturity));
                    break;
                case VolUpdateTempos.FifteenMinutes:
                    oneDayMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(1);
                    var oneWeekMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(7);
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneDayMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneWeekMaturity));
                    break;
                case VolUpdateTempos.ThirtyMinutes:
                    oneDayMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(1);
                    oneWeekMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(7);
                    var twoWeeksMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(14);
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneDayMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneWeekMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, twoWeeksMaturity));
                    break;
                case VolUpdateTempos.OneHour:
                    oneDayMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(1);
                    oneWeekMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(7);
                    twoWeeksMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(14);
                    var threeWeeksMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(21);
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneDayMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneWeekMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, twoWeeksMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, threeWeeksMaturity));
                    break;
                case VolUpdateTempos.TwoHours:
                    oneDayMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(1);
                    oneWeekMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(7);
                    twoWeeksMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(14);
                    threeWeeksMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(21);
                    var oneMonthMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(30);
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneDayMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneWeekMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, twoWeeksMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, threeWeeksMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneMonthMaturity));
                    break;
                case VolUpdateTempos.FourHours:
                    oneDayMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(1);
                    oneWeekMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(7);
                    twoWeeksMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(14);
                    threeWeeksMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(21);
                    oneMonthMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(30);
                    var twoMonthsMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(60);
                    var threeMonthsMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(90);
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneDayMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneWeekMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, twoWeeksMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, threeWeeksMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, oneMonthMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, twoMonthsMaturity));
                    requiredSmiles.AddRange(Tools.ExtractSmiles(allSmiles.Values, threeMonthsMaturity));
                    break;
                default:
                    throw new NotImplementedException($"Unknown tempo {updateTempo} for calibration.");
            }
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
                        res.TryAdd(smile.Value.SmileDate, Futures.GetNoExpiryFutures(smile.Value.SmileDate, Pair));

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
