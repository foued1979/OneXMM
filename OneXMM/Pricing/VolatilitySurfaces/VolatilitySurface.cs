using System;

namespace OneXMM.Pricing.VolatilitySurfaces
{
    using OneXMM.Interfaces;
    using OneXMM.Math.Extrapolators;
    using OneXMM.Math.Interpolators;
    using OneXMM.Math.Optimizers;
    using OneXMM.Pricing.Curves;
    using OneXMM.Pricing.Smiles;
    using OneXMM.Referential;
    using OneXMM.Referential.Assets;

    public sealed class VolatilitySurface : IVolatilitySurface
    {
        private RateInterpolationTypes _rateInterpolationType;
        private SemaphoreSlim _synchronizer = new SemaphoreSlim(1, 1);

        public Pair Pair { get; }
        public DateTime SurfaceDate { get; private set; }
        public ISmile[] Smiles { get; private set; }
        public ICurve ForwardCurve { get; private set; }
        public double SpotRef { get; private set; }

        public VolatilitySurface(Pair pair, DateTime asof, double spot, Futures[] futures, Dictionary<Pair, SortedList<DateTime, Dictionary<Option, double>>> optionQuotes, RateInterpolationTypes rateInterpolationType)
        {
            Pair = pair;
            SurfaceDate = asof;
            _rateInterpolationType = rateInterpolationType;

            UpdateSurface(asof, spot, futures, optionQuotes);
        }

        public void UpdateSurface(DateTime asof, double spot, Futures[] futures, Dictionary<Pair, SortedList<DateTime, Dictionary<Option, double>>> optionQuotes)
        {
            var quotes = optionQuotes[Pair];
            var tasks = new Task<bool>[quotes.Count];
            var index = 0;

            _synchronizer.Wait();

            ForwardCurve = new Curve<PiecewiseLinearRateInterpolator<FlatRateExtrapolator, LinearRateExtrapolator>>(asof, futures, _rateInterpolationType);

            var tmpSmiles = new ISmile[quotes.Count];
            SpotRef = spot;

            foreach (var kv in quotes)
            {
                ISmile newSmile = new SABR(asof, kv.Key);
                tmpSmiles[index] = newSmile;

                tasks[index] = Task<bool>.Run(() =>
                {
                    var expiry = (newSmile.SmileExpiry - asof).TotalDays * Constants.OneOver365;
                    return SmileOptimizer.CalibrateSmile(spot, ForwardCurve.GetForward(spot, expiry), ref newSmile, quotes[newSmile.SmileExpiry]);
                });
            }

            Task.WaitAll(tasks);

            if (!tasks.All(t => t.Result)) return;
            Smiles = tmpSmiles;

            _synchronizer.Release();
        }
    }
}
