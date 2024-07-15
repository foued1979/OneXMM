using System;

namespace OneXMM.Pricing
{
    using OneXMM.Referential.Assets;
    using OneXMM.Referential;
    using OneXMM.Utils;
    using OneXMM.Referential.Exchanges;
    using System.Reflection;
    using OneXMM.Interfaces;

    public class VolatilitySurface
    {
        private SortedDictionary<string, Futures> _futuresBySymbol = new SortedDictionary<string, Futures>();
        private SortedDictionary<DateTime, Futures> _futures = new SortedDictionary<DateTime, Futures>();
        private SortedDictionary<DateTime, SABR> _smiles = new SortedDictionary<DateTime, SABR>();

        public Pair Pair { get; }
        public IExchange Exchange { get; }
        public SABR[] Smiles => _smiles.Values.ToArray();
        public DateTime SurfaceDate { get; }

        public VolatilitySurface(DateTime surfaceDate, Pair pair, IExchange exchange)
        {
            Pair = pair;
            Exchange = exchange;
            SurfaceDate = surfaceDate; 
        }

        public void Calibrate(VolUpdateTempos updateTempo)
        {
            var spot = Exchange.GetSpot(Pair);
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

            var SABRToCalibrate = new Dictionary<SABR, Option[]>(smiles.Count);

            for (int i = 0; i < smiles.Count; ++i)
            {
                var smile = smiles.Values.ElementAt(i);
                SABRToCalibrate.Add(smile, smile.Options.SelectOptions(spot));
            }

            var waitingTime = new TimeSpan(0, 0, 0, 1, 10);

            for (int i = 0; i < SABRToCalibrate.Count; ++i)
            {
                var startTime = DateTime.Now;

                var prices = Exchange.GetOptionPrices(SABRToCalibrate.ElementAt(i).Value);
            }
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

        public double GetForward(double spot, DateTime asof, DateTime expiry)
        {
            var yf = 0.0;

            if (_futures.First().Key.CompareTo(expiry) > 0)
            {
                yf = (_futures.First().Key - asof).TotalDays / 365.0;
                return spot * (1 + _futures.First().Value.Yield * yf);
            }

            var expiries = _futures.Keys;
            yf = (expiry - asof).TotalDays / 365.0;

            for (int i = 0; i < _futures.Count; i++)
            {
                if (expiry.IsAround(expiries.ElementAt(i)))
                {
                    return spot * (1 + _futures.ElementAt(i).Value.Yield * yf);
                }
                else if (expiry.CompareTo(expiries.ElementAt(i)) < 0)
                {
                    var tenorDown = (_futures.ElementAt(i - 1).Key - asof).TotalDays / 365.0;
                    var yieldDown = _futures.ElementAt(i - 1).Value.Yield;

                    var tenorUp = (_futures.ElementAt(i).Key - asof).TotalDays / 365.0;
                    var yieldUp = _futures.ElementAt(i).Value.Yield;

                    return yieldDown + (yf - tenorDown) / (tenorUp - tenorDown) * (yieldUp - yieldDown);
                }
            }

            return spot * (1 + _futures.Last().Value.Yield * yf);
        }

        public double GetVol(double forward, double strike, DateTime expiry)
        {
            if (_smiles.Keys.First().CompareTo(expiry) > 0)
            {
                return _smiles.First().Value.GetVol(forward, strike);
            }

            var expiries = _smiles.Keys;

            for (int i = 0; i < _smiles.Count; ++i)
            {
                if (expiry.IsAround(expiries.ElementAt(i)))
                {
                    return _smiles.ElementAt(i).Value.GetVol(forward, strike);
                }
                else if (expiry.CompareTo(expiries.ElementAt(i)) < 0)
                {
                    var one_over_365 = 1.0 / 365.0;
                    var smileDown = _smiles.ElementAt(i - 1).Value;
                    var smileUp = _smiles.ElementAt(i).Value;

                    var volDn = smileDown.GetVol(forward, strike);
                    var volUp = smileUp.GetVol(forward, strike);

                    var yf = (expiry - SurfaceDate).TotalDays * one_over_365;
                    var r = (expiry - smileDown.SmileExpiry).TotalDays * one_over_365;
                    var l = (smileUp.SmileExpiry - smileDown.SmileExpiry).TotalDays * one_over_365;
                    var varianceDn = volDn * volDn * (smileDown.SmileExpiry - SurfaceDate).TotalDays * one_over_365;
                    var varianceUp = volUp * volUp * (smileUp.SmileExpiry - SurfaceDate).TotalDays * one_over_365;

                    return System.Math.Sqrt((varianceDn + r / l * (varianceUp - varianceDn)) / yf);
                }
            }

            return _smiles.Last().Value.GetVol(forward, strike);
        }
    }
}
