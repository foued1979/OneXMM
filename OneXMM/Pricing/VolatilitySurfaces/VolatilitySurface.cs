using Accord;
using Accord.Math.Optimization;
using System;
using System.Runtime.CompilerServices;

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
        private LinearSmileInterpolator _linearSmileInterpolator;

        public Pair Pair { get; }
        public DateTime SurfaceDate { get; private set; }
        public ISmile[] Smiles { get; private set; } = [];
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
            _linearSmileInterpolator = new LinearSmileInterpolator(tmpSmiles, ForwardCurve);

            _synchronizer.Release();
        }

        public double GetForward(double spot, DateTime expiry)
        {
            return ForwardCurve.GetForward(spot, expiry);
        }

        public double GetVol(double spot, double strike, DateTime expiry)
        {
            if (expiry.CompareTo(Smiles[0].SmileExpiry) <= 0) return Smiles[0].GetVol(ForwardCurve.GetForward(spot, Smiles[0].Expiry), strike);
            var comparedToLastSmile = expiry.CompareTo(Smiles.Last().SmileExpiry);
            if (comparedToLastSmile == 0) return Smiles.Last().GetVol(ForwardCurve.GetForward(spot, Smiles.Last().Expiry), strike);
            if (comparedToLastSmile > 0) return _linearSmileInterpolator.Extrapolate(spot, strike, expiry);

            LocateSmile(expiry, out var prevSmile, out var nextSmile);

            var exp = (expiry - SurfaceDate).TotalDays * Constants.OneOver365;
            var sqrt_exp = System.Math.Sqrt(exp);
            var prop = (exp - prevSmile.Expiry) / (nextSmile.Expiry - prevSmile.Expiry);
            var fwd = ForwardCurve.GetForward(spot, expiry);
            var volApprox = GetVolApproxFunction(prop, ForwardCurve.GetForward(spot, prevSmile.Expiry), prevSmile, ForwardCurve.GetForward(spot, nextSmile.Expiry), nextSmile);

            var brent = new BrentSearch((moneyness) =>
            {
                var approxVol = volApprox(moneyness);
                return moneyness - System.Math.Log(strike / fwd) / (approxVol * sqrt_exp);
            }, -20, 20, 1E-8);

            if (!brent.FindRoot()) throw new Exception($"VolatilitySurface - Cannot find volatility for Pair {Pair.Symbol} as of {SurfaceDate.ToString("dd/MM/yyyy HH:mm:ss")}.");
            return brent.Solution;
        }

        public string[] ToFields()
        {
            throw new NotImplementedException();
        }

        private void LocateSmile(DateTime expiry, out ISmile prevSmile, out ISmile nextSmile)
        {
            var index = 0;

            foreach (var smile in Smiles)
            {
                if (smile.SmileExpiry.CompareTo(expiry) > 0)
                {
                    nextSmile = smile;
                    prevSmile = Smiles[index - 1];
                    return;
                }

                index++;
            }

            throw new Exception();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Func<double, double> GetVolApproxFunction(double prop, double prevForward, ISmile prevSmile, double nextForward, ISmile nextSmile)
        {
            return new Func<double, double>((moneyness) =>
            {
                var prevVol = prevSmile.GetVolFromMoneyness(prevForward, moneyness);
                var nextVol = nextSmile.GetVolFromMoneyness(nextForward, moneyness);
                return prevVol + prop * (nextVol - prevVol);
            });
        }
    }
}
