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
            var smiles = new Dictionary<DateTime, SABR>(_smiles.Count);
            var now = DateTime.Now;

            switch (updateTempo)
            {
                case VolUpdateTempos.OneHour:
                    var oneDayMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(1);
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, oneDayMaturity));
                    break;
                case VolUpdateTempos.TwoHours:
                    oneDayMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(1);
                    var oneWeekMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(7);
                    var twoWeeksMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(14);
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, oneDayMaturity));
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, oneWeekMaturity));
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, twoWeeksMaturity));
                    break;
                case VolUpdateTempos.FourHours:
                    oneDayMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(1);
                    oneWeekMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(7);
                    twoWeeksMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(14);
                    var threeWeeksMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(14);
                    var oneMonthMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(14);
                    var twoMonthsMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(14);
                    var threeMonthsMaturity = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(14);
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, oneDayMaturity));
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, oneWeekMaturity));
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, twoWeeksMaturity));
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, threeWeeksMaturity));
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, oneMonthMaturity));
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, twoMonthsMaturity));
                    smiles.AddRange(Tools.ExtractSmiles(_smiles.Values, threeMonthsMaturity));
                    break;
                default:
                    throw new NotImplementedException($"Unknown tempo {updateTempo} for calibration.");
            }

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

            var calibrationTasks = new Task<MinimizationResult>[SABRToCalibrate.Count];
            var prices = new SortedDictionary<Option, double>[SABRToCalibrate.Count];
            var result = new SABR[SABRToCalibrate.Count];

            spot = Exchange.GetSpot(Pair);
            var calibrationTimestamp = DateTime.Now;
            Thread.Sleep(1000);

            for (int i = 0; i < SABRToCalibrate.Count; ++i)
            {
                var index = i;

                var elementsToCalibrate = SABRToCalibrate.ElementAt(i).Value;
                var forward = GetForward(spot, calibrationTimestamp, SABRToCalibrate.Keys.ElementAt(i).SmileExpiry);

                var stopTime = DateTime.Now.AddSeconds(1);
                prices[index] = Exchange.GetOptionPrices(elementsToCalibrate);

                calibrationTasks[index] = Task<MinimizationResult>.Run(() =>
                {
                    return VolOptimizer.CalibrateSmile(spot, forward, SABRToCalibrate.ElementAt(index).Key, prices[index]);
                });

                while (DateTime.Now.CompareTo(stopTime) < 0) ;
            }

            Task.WaitAll(calibrationTasks);

            for (int i = 0; i < SABRToCalibrate.Count; ++i)
            {
                var parameters = calibrationTasks[i].Result.MinimizingPoint;
                result[i] = SABRToCalibrate.ElementAt(i).Key.WithParameters(parameters[0], parameters[1], parameters[2], parameters[3]);
            }

            foreach (var sabr in result)
            {
                Smiles.Add(sabr.SmileExpiry, sabr);
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
