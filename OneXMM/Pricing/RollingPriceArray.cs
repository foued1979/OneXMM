using System;

namespace OneXMM.Pricing
{
    using OneXMM.Referential.Assets;

    public sealed class RollingPriceArray
    {
        private readonly Pair _pair;
        private int _indexLast;
        private readonly double[] _values;

        public int Capacity { get; }

        public RollingPriceArray(Pair pair, int capacity)
        {
            _pair = pair;
            _values = new double[capacity];
            Capacity = capacity;
        }

        public void Add(double price)
        {
            if (_indexLast == _values.Length) _indexLast = 0;
            else _indexLast++;

            _values[_indexLast] = price;
        }

        public double[] ExtractPrices(int frequency, int size)
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

                if (hasAlreadyLooped && index <= _indexLast) throw new Exception($"RollingPriceArray - Capacity ({Capacity}) is not properly dimensioned for pair {_pair.Symbol} with frequency {frequency}.");
            }

            return res;
        }

        public double[] ExtractReturns(int frequency, int size)
        {
            var res = new double[size];
            var index = _indexLast;
            var local = _values;
            var hasAlreadyLooped = false;

            for (int i = size - 1; i >= 0; i--)
            {
                var nextIndex = index - frequency;

                if (nextIndex < 0)
                {
                    nextIndex += local.Length;
                    hasAlreadyLooped = true;
                }

                if (hasAlreadyLooped && nextIndex <= _indexLast) throw new Exception($"RollingPriceArray - Capacity ({Capacity}) is not properly dimensioned for pair {_pair.Symbol} with frequency {frequency}.");

                res[i] = local[index] / local[nextIndex] - 1.0;
                index = nextIndex;
            }

            return res;
        }
    }
}
