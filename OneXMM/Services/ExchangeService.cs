using System;
using System.Runtime.CompilerServices;

namespace OneXMM.Services
{
    using OneXMM.Communication;
    using OneXMM.Communication.MySQLRequests;
    using OneXMM.Interfaces;
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.DataFeeds;
    using OneXMM.Referential.Exchanges;
    using OneXMM.Services.ServiceEventArgs;
    using OneXMM.Utils;
    using System.Collections.Concurrent;

    public static class ExchangeService
    {
        #region Events
        public static event EventHandler<Pair> FuturesAndOptionsUpdateFailed;
        public static event EventHandler<FuturesUpdatedEventArgs> FuturesUpdated;
        public static event EventHandler<OptionsUpdatedEventArgs> OptionsUpdated;
        #endregion

        public static bool IsInitiated { get; private set; } = false;
        public static DateTime ReferentialDate { get; set; } = DateTime.Today;
        public readonly static SortedDictionary<Pair, (Futures[], SortedList<DateTime, IEnumerable<Option>>)> FilteredOptions = new SortedDictionary<Pair, (Futures[], SortedList<DateTime, IEnumerable<Option>>)>();

        #region Private and Internal Fields
        private readonly static IDBConnector _sqlConnector = Factory.CreateMySQLConnector();
        internal readonly static Dictionary<Exchanges, Exchange> _exchanges;
        internal readonly static SortedDictionary<Pair, (Futures[], SortedList<DateTime, IEnumerable<Option>>)> _instrumentsByPair = new SortedDictionary<Pair, (Futures[], SortedList<DateTime, IEnumerable<Option>>)>();
        #endregion

        static ExchangeService()
        {
            if (!DataFeederService.IsInitialized) throw new Exception("ExchangeService - DataFeederService must be initialized first.");

            var allExchanges = Enum.GetValues(typeof(Exchanges));
            _exchanges = new Dictionary<Exchanges, Exchange>(allExchanges.Length);

            foreach (Exchanges exchange in allExchanges)
            {
                if (exchange == Exchanges.None) continue;
                AddExchange(exchange);
            }

            IsInitiated = true;

            var todayStr = ReferentialDate.ToString(MySQLConnector.DateFormat);

            foreach (var pair in DataFeederService.Pairs)
            {
                // For correlated vol, there is no instrument to load for the pair
                if (pair.CorrelatedVol) continue;

                var exchange = _exchanges[Config.GetReferenceExchangeForPair(pair)];

                var getFuturesRequest = new GetFuturesRequest(ReferentialDate, pair);
                var futuresResult = _sqlConnector.Get(getFuturesRequest);
                Futures[] futures = [];

                if (futuresResult.Count() == 0) UpdateFutures(pair, out futures);
                else futures = futuresResult.Select(r => r.ToFutures(pair)).ToArray();

                var getOptionRequest = new GetOptionsRequest(ReferentialDate, pair);
                var optionsResult = _sqlConnector.Get(getOptionRequest);
                SortedList<DateTime, IEnumerable<Option>> options = new SortedList<DateTime, IEnumerable<Option>>();

                if (optionsResult.Count() == 0) UpdateOptions(pair, out options);
                else options = optionsResult.Select(r => r.ToOption(pair)).FilterByExpiry();

                _instrumentsByPair.Add(pair, (futures, options));
                if (getFuturesRequest.LatestInputDate.CompareTo(ReferentialDate) < 0 || getOptionRequest.LatestInputDate.CompareTo(ReferentialDate) < 0) IsInitiated &= UpdateOptionAndFuturesReferential(pair);
            }
        }

        public static void AddExchange(Exchanges exchange)
        {
            if (_exchanges.ContainsKey(exchange)) return;

            switch (exchange)
            {
                case Exchanges.Deribit:
                    _exchanges.Add(exchange, new Deribit(Config.IsTestMode));
                    break;
                default:
                    throw new NotImplementedException($"Unknown {exchange}.");
            }
        }

        public static void FilterOptions(IEnumerable<Quote> quotes)
        {
            var tasks = new Task[quotes.Count()];
            int index = 0;

            foreach (var quote in quotes)
            {
                tasks[index] = Task.Run(() =>
                {
                    if (_instrumentsByPair.TryGetValue(quote.Pair, out var futuresAndOptions))
                    {
                        var exchange = futuresAndOptions.Item1[0].Exchange;
                        var queryTime = DateTime.Now;

                        var futuresPrices = exchange.GetSpot(futuresAndOptions.Item1);
                        var index = 0;

                        var filteredOptions = new SortedList<DateTime, IEnumerable<Option>>(futuresAndOptions.Item1.Length);

                        foreach (var fut in futuresAndOptions.Item1)
                        {
                            var futVal = futuresPrices[index];
                            fut.SetYield(queryTime, quote.Price, futVal);
                            var selectedOptions = new List<Option>(5);

                            filteredOptions.Add(fut.Expiry, GetFilteredOptions(quote.Pair, exchange, futVal, futuresAndOptions.Item2[fut.Expiry]));
                            index++;
                        }

                        FilteredOptions[quote.Pair] = (futuresAndOptions.Item1, filteredOptions); 
                    }
                });
            }

            Task.WaitAll(tasks);
        }

        public static Dictionary<Pair, SortedList<DateTime, Dictionary<Option, double>>> GetOptionPrices()
        {
            var tasks = new Task[FilteredOptions.Count];
            var index = 0;

            var res = new ConcurrentDictionary<Pair, SortedList<DateTime, Dictionary<Option, double>>>();

            foreach (var instrByPair in FilteredOptions)
            {
                tasks[index] = Task.Run(() =>
                {
                    var options = instrByPair.Value.Item2;
                    var exchange = _exchanges[Config.GetReferenceExchangeForPair(instrByPair.Key)];

                    res.AddOrUpdate(instrByPair.Key, exchange.GetOptionPrices(options), (p, v) => { return v; });
                });

                index++;
            }

            Task.WaitAll(tasks);
            return res.ToDictionary();
        }

        public static void UpdateOptionAndFuturesReferential(IEnumerable<Pair> pairs)
        {
            foreach (var pair in pairs)
            {
                var tasks = new Task[2];
                var success = true;

                tasks[0] = Task.Run(() => success = UpdateOptionAndFuturesReferential(pair));
                tasks[1] = Task.Delay(500);

                Task.WaitAll(tasks);
                if (!success) FuturesAndOptionsUpdateFailed(null, pair);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool UpdateOptionAndFuturesReferential(Pair pair)
        {
            var tasks = new Task<bool>[2];

            try
            {
                tasks[0] = Task.Run(() => UpdateFutures(pair, out var futures));
                tasks[1] = Task.Run(() => UpdateOptions(pair, out var options));
            }
            catch
            {
                FuturesAndOptionsUpdateFailed(null, pair);
                return false;
            }

            Task.WaitAll(tasks);

            ReferentialDate = DateTime.Today;
            return tasks[0].Result && tasks[1].Result;
        }

        public static bool UpdateFutures(Pair pair, out Futures[] futures)
        {
            if (pair.CorrelatedVol) { futures = []; return true; }

            var todayStr = DateTime.Today.ToString(MySQLConnector.DateFormat);
            var exchange = _exchanges[Config.GetReferenceExchangeForPair(pair)];

            // We load the futures from exchange
            try
            {
                futures = exchange.LoadFutures(pair);
            }
            catch
            {
                futures = [];
                return false;
            }

            // We filter the futures
            if (!_instrumentsByPair.TryGetValue(pair, out var currentOptionsAndFutures))
            {
                // We load the existing futures, and we remove the expired ones
                var currentAliveFutures = currentOptionsAndFutures.Item1.Where(f => f.Expiry.CompareTo(DateTime.Now) > 0);

                // We filter the new futures
                var futuresToAdd = futures.Where(f => (f.Expiry - DateTime.Now).TotalDays * Constants.OneOver365 <= Config.MaxEpiry
                    && !currentAliveFutures.Any(cf => cf.Expiry.CompareTo(f.Expiry) == 0));
                var nbNewFutures = futuresToAdd.Count();

                if (nbNewFutures != 0)
                {
                    foreach (var newFutures in futuresToAdd) currentAliveFutures = currentAliveFutures.Append(newFutures);
                    futures = currentAliveFutures.ToArray();

                    lock (_instrumentsByPair)
                    {
                        _instrumentsByPair[pair] = (futures, currentOptionsAndFutures.Item2);
                    }

                    // We throw an event to generate update of vol surfaces
                    FuturesUpdated.Invoke(null, new FuturesUpdatedEventArgs(pair, exchange));

                    // We update the database
                    string[][] dataToInsert = new string[nbNewFutures][];

                    for (int i = 0; i < nbNewFutures; ++i)
                    {
                        dataToInsert[i] = futuresToAdd.ElementAt(i).ToFields();
                        dataToInsert[i] = dataToInsert[i].Append(todayStr).ToArray();
                    }

                    var insertReq = new InsertDataRequest(MySQLTables.futures_ref.ToString(), dataToInsert, MySQLInsertTypes.IdNotInDataToInsert);
                    _sqlConnector.InsertData(insertReq);
                }
                else
                {
                    futures = currentAliveFutures.ToArray();
                }
            }

            return true;
        }

        public static bool UpdateOptions(Pair pair, out SortedList<DateTime, IEnumerable<Option>> options)
        {
            // For correlated vol, there is no instrument to load for the pair
            if (pair.CorrelatedVol) { options = []; return true; }

            var todayStr = DateTime.Today.ToString(MySQLConnector.DateFormat);
            var exchange = _exchanges[Config.GetReferenceExchangeForPair(pair)];

            // We load the options from the exchange
            try
            {
                options = exchange.LoadOptions(pair).FilterByExpiry();
            }
            catch
            {
                options = [];
                return false;
            }

            // We filter the futures
            if (!_instrumentsByPair.TryGetValue(pair, out var currentOptionsAndFutures))
            {
                // We load the existing options, and we remove the expired ones
                var currentAliveOptions = currentOptionsAndFutures.Item2.Where(f => f.Key.CompareTo(DateTime.Now) > 0);

                // We filter the new options
                var optionsToAdd = options.Where(o => (o.Key - DateTime.Now).TotalDays * Constants.OneOver365 <= Config.MaxEpiry);
                var selectedOptionsToAdd = new SortedList<DateTime, IEnumerable<Option>>();

                foreach (var kv in optionsToAdd)
                {
                    var allOptionsWithSameExpiry = currentAliveOptions.Where(cao => cao.Key.CompareTo(kv.Key) == 0);
                    if (allOptionsWithSameExpiry is null || allOptionsWithSameExpiry.Count() == 0) 
                    {
                        selectedOptionsToAdd.Add(kv.Key, new List<Option>(kv.Value.Count()));
                        foreach (var option in kv.Value)
                        {
                            selectedOptionsToAdd[kv.Key].Append(option);
                        }
                        continue;
                    }

                    var optionsWithSameExpiry = allOptionsWithSameExpiry.First().Value;

                    foreach (var optionToAdd in kv.Value)
                    {
                        if (optionsWithSameExpiry.Any(ose => ose.Strike == optionToAdd.Strike)) continue;
                        selectedOptionsToAdd[kv.Key].Append(optionToAdd);
                    }
                }

                var nbNewOptions = selectedOptionsToAdd.Count();

                if (nbNewOptions != 0)
                {
                    options = new SortedList<DateTime, IEnumerable<Option>>();
                    List<string[]> dataToInsert = new List<string[]>();

                    foreach (var kv in optionsToAdd)
                    {
                        options.Add(kv.Key, kv.Value);
                    }

                    foreach (var kv in selectedOptionsToAdd)
                    {
                        foreach (var o in kv.Value) { options[kv.Key].Append(o); dataToInsert.Add(o.ToFields().Append(todayStr).ToArray()); }
                    }

                    lock (_instrumentsByPair)
                    {
                        _instrumentsByPair[pair] = (currentOptionsAndFutures.Item1, options);
                    }

                    // We throw an event to generate update of vol surfaces
                    OptionsUpdated.Invoke(null, new OptionsUpdatedEventArgs(pair, exchange));

                    // We update the database
                    var insertReq = new InsertDataRequest(MySQLTables.options_ref.ToString(), dataToInsert.ToArray(), MySQLInsertTypes.IdNotInDataToInsert);
                    _sqlConnector.InsertData(insertReq);
                }
                else
                {
                    options = currentAliveOptions.ToSortedList();
                }
            }

            return true;
        }

        public static Exchange GetExchange(Exchanges exchange) => _exchanges[exchange];

        /// <summary>
        /// Returns the reference exchange for the pair.
        /// </summary>
        public static Exchange GetExchange(Pair pair)
        {
            var pairDef = Config._referential.PairDefs.First(pd => pd.CoinId == pair.Coin.Id && pd.BaseId == pair.Base.Id);
            return _exchanges[pairDef.RefExchange];
        }

        public static bool GetRefInstrumentsForPair(Pair pair, out (Futures[], SortedList<DateTime, IEnumerable<Option>>) refInstruments)
        {
            if (_instrumentsByPair.TryGetValue(pair, out refInstruments)) return true;
            refInstruments = ([], []);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetSpot(Pair pair)
        {
            return _exchanges[Config.GetReferenceExchangeForPair(pair)].GetSpot(pair);
        }

        private static Option[] GetFilteredOptions(Pair pair, IExchange exchange, double futuresPrice, IEnumerable<Option> allOptionsForExpiry)
        {
            var selectedStrike = allOptionsForExpiry.First();
            var minDist = System.Math.Abs(selectedStrike.Strike - futuresPrice);
            var atmIndex = 0;

            for (int i = 1; i < allOptionsForExpiry.Count(); ++i)
            {
                var currentOption = allOptionsForExpiry.ElementAt(i);
                var dist = System.Math.Abs(currentOption.Strike - futuresPrice);
                if (dist < minDist) { minDist = dist; atmIndex = i; selectedStrike = currentOption; }
            }

            if (atmIndex == 0) throw new Exception($"ExchangeService::GetFilteredOptions - Error with option selection for pair {pair.Symbol}, on exchange {exchange.Name}, for expiry {selectedStrike.Expiry.ToShortDateString()}.");
            if (atmIndex == 1)
            {
                if (allOptionsForExpiry.Count() < 2) throw new Exception($"ExchangeService::GetFilteredOptions - Not enough options for pair {pair.Symbol}, on exchange {exchange.Name}, for expiry {selectedStrike.Expiry.ToShortDateString()}.");
                return [allOptionsForExpiry.ElementAt(0), allOptionsForExpiry.ElementAt(1), allOptionsForExpiry.ElementAt(2)];
            }
            if (atmIndex == 2)
            {
                if (allOptionsForExpiry.Count() < 4) throw new Exception($"ExchangeService::GetFilteredOptions - Not enough options for pair {pair.Symbol}, on exchange {exchange.Name}, for expiry {selectedStrike.Expiry.ToShortDateString()}.");
                if (allOptionsForExpiry.Count() == 4) return [allOptionsForExpiry.ElementAt(0), allOptionsForExpiry.ElementAt(1), allOptionsForExpiry.ElementAt(2), allOptionsForExpiry.ElementAt(3)];
                else return [allOptionsForExpiry.ElementAt(0), allOptionsForExpiry.ElementAt(1), allOptionsForExpiry.ElementAt(2), allOptionsForExpiry.ElementAt(3), allOptionsForExpiry.ElementAt(4)];
            }

            var atm_m1 = (int)System.Math.Floor(0.5 * atmIndex);
            var atm_m2 = (int)System.Math.Floor(0.5 * atm_m1);

            var dUp = allOptionsForExpiry.Count() - atmIndex;
            var atm_p1 = atmIndex + (int)System.Math.Floor(0.5 * dUp);
            var atm_p2 = allOptionsForExpiry.Count() - (int)System.Math.Floor(0.5 * (allOptionsForExpiry.Count() - atm_p1));

            if (atm_p1 == atm_p2) return [allOptionsForExpiry.ElementAt(atm_m2), allOptionsForExpiry.ElementAt(atm_m1), allOptionsForExpiry.ElementAt(atmIndex), allOptionsForExpiry.ElementAt(atm_p1)];
            else return [allOptionsForExpiry.ElementAt(atm_m2), allOptionsForExpiry.ElementAt(atm_m1), allOptionsForExpiry.ElementAt(atmIndex), allOptionsForExpiry.ElementAt(atm_p1), allOptionsForExpiry.ElementAt(atm_p2)];
        }
    }
}
