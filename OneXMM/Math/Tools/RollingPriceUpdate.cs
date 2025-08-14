using System;

namespace OneXMM.Math.Tools
{
    using OneXMM.Communication.MySQLResults;
    using OneXMM.Referential.Assets;
    using OneXMM.Referential.DataFeeds;

    public sealed class RollingPriceUpdate
    {
        private DateTime _asof;
        private readonly Pair _pair;
        private int _indexLast;
        private readonly double[] _values;
        private readonly int _capacity;

        private readonly SortedDictionary<int, RealtimePriceData> _data = new SortedDictionary<int, RealtimePriceData>();

        public double[] GetReturns(int frequency)
        {
            if (_data.TryGetValue(frequency, out var data)) return data.ReturnData;
            return [];
        }

        public RollingPriceUpdate(Pair pair, int capacity, IEnumerable<HistoricalResult> historicalQuotes)
        {
            _pair = pair;
            _values = new double[capacity];
            _capacity = capacity;

            for (int i = 0; i < historicalQuotes.Count(); ++i)
            {
                _values[_indexLast] = historicalQuotes.ElementAt(i).Value;

                _indexLast++;
                if (_indexLast == _values.Length) _indexLast = 0;
            }

            _asof = historicalQuotes.Last().Timestamp;
        }

        public RollingPriceUpdate(Pair pair, int capacity, IEnumerable<HistoricalQuote> historicalQuotes)
        {
            _pair = pair;
            _values = new double[capacity];
            _capacity = capacity;

            for (int i = 0; i < historicalQuotes.Count(); ++i)
            {
                _values[_indexLast] = historicalQuotes.ElementAt(i).Price;

                _indexLast++;
                if (_indexLast == _values.Length) _indexLast = 0;
            }

            _asof = historicalQuotes.Last().Timestamp;
        }

        public void AddHistoricalQuotes(IEnumerable<HistoricalQuote> historicalQuotes)
        {
            for (int i = 0; i < historicalQuotes.Count(); ++i)
            {
                _values[_indexLast] = historicalQuotes.ElementAt(i).Price;

                _indexLast++;
                if (_indexLast == _values.Length) _indexLast = 0;
            }

            _asof = historicalQuotes.Last().Timestamp;
        }

        public void AddAnalytics(int frequency, int size)
        {
            if (_data.TryGetValue(frequency, out var priceData))
            {
                if (priceData.Size == size) return;

                var newData = ExtractPrices(frequency, size);
                lock (_data) _data[frequency] = new RealtimePriceData(_asof, newData);
            }
            else
            {
                var newData = ExtractPrices(frequency, size);
                lock (_data) _data.Add(frequency, new RealtimePriceData(_asof, newData));
            }
        }

        public void Update(double price)
        {
            foreach (var rpd in _data.Values)
            {
                rpd.Update(DateTime.Today, price);
            }
        }

        public double[] ExtractReturns(int frequency, int size)
        {
            lock (_data)
            {
                if (_data.TryGetValue(frequency, out var realtimePriceData)) return realtimePriceData.ReturnData;
                AddAnalytics(frequency, size);
                return _data[frequency].ReturnData;
            }
            
        }

        private double[] ExtractPrices(int frequency, int size)
        {
            var res = new double[size];
            var index = _indexLast;
            var local = _values;
            var hasAlreadyLooped = false;

            for (int i = size - 1; i >= 0; i--)
            {
                res[i] = local[index];
                index -= frequency;

                if (index < 0)
                {
                    index += local.Length;
                    hasAlreadyLooped = true;
                }

                if (hasAlreadyLooped && index <= _indexLast) throw new Exception($"RollingPriceArray - Capacity ({_capacity}) is not properly dimensioned for pair {_pair.Symbol} with frequency {frequency}.");
            }

            return res;
        }
    }
}
