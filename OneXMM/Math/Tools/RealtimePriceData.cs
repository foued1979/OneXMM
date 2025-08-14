using System;

namespace OneXMM.Math.Tools
{
    internal sealed class RealtimePriceData
    {
        private SemaphoreSlim _sync = new SemaphoreSlim(1, 1);
        private DateTime _asof;
        private readonly double[] _priceData;
        private readonly double[] _returnData;
        private int _prevPriceIndex;
        private int _lastPriceIndex;
        private int _lastReturnIndex;

        public int Size => _returnData.Length;
        public double[] ReturnData => _returnData;

        public RealtimePriceData(DateTime asof, double[] selectedPrices)
        {
            _asof = asof;
            _priceData = selectedPrices;
            _returnData = new double[selectedPrices.Length - 1];

            for (int i = 1; i < selectedPrices.Length; i++) _returnData[i - 1] = selectedPrices[i] / selectedPrices[i - 1] - 1.0;

            _lastPriceIndex = selectedPrices.Length - 1;
            _prevPriceIndex = _lastPriceIndex - 1;
            _lastReturnIndex = _lastPriceIndex - 1;
        }

        public void Update(DateTime asof, double lastPrice)
        {
            _sync.Wait();

            if (asof.DayOfYear != _asof.DayOfYear)
            {
                _asof = asof;
                _prevPriceIndex++;
                _lastPriceIndex++;
                _lastReturnIndex++;

                if (_prevPriceIndex >= _priceData.Length) _prevPriceIndex = 0;
                if (_lastPriceIndex >= _priceData.Length) _lastPriceIndex = 0;
                if (_lastReturnIndex >= _priceData.Length) _lastReturnIndex = 0;
            }

            _priceData[_lastPriceIndex] = lastPrice;
            _returnData[_lastReturnIndex] = lastPrice / _priceData[_prevPriceIndex] - 1.0;

            _sync.Release();
        }
    }
}
