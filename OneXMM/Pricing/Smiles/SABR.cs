using System;
using System.Runtime.CompilerServices;

namespace OneXMM.Pricing.Smiles
{
    using OneXMM;
    using OneXMM.Interfaces;
    using OneXMM.Referential;

    public sealed class SABR : ISmile
    {
        public DateTime SmileDate { get; private set; }
        public DateTime SmileExpiry { get; }
        
        public double Alpha { get; set; }
        public double Vega { get; set; }
        public double Beta { get; set; }
        public double Rho { get; set; }

        private SortedDictionary<string, Option> _optionsBySymbol = new SortedDictionary<string, Option>();
        private SortedDictionary<double, Option> _options = new SortedDictionary<double, Option>();

        public SortedDictionary<double, Option> GetOptions() => _options;

        public SABR(DateTime smileDate, DateTime smileExpiry)
        {
            SmileDate = smileDate;
            SmileExpiry = smileExpiry;
        }

        public void UpdateParameters(params double[] args)
        {
            UpdateParameters(args[0], args[1], args[2], args[3]);
        }

        public void UpdateParameters(double alpha, double vega, double beta, double rho)
        {
            Alpha = alpha;
            Vega = vega;
            Beta = beta;
            Rho = rho;
        }

        public void UpdateSmileDate(DateTime smileDate)
        {
            SmileDate = smileDate;
        }

        public void AddOption(Option option)
        {
            if (option.Expiry.CompareTo(SmileExpiry) != 0) throw new Exception($"Wrong option attached to SABR smile for {option.Pair.Symbol}.");

            lock (_options)
            {
                if (_options.ContainsKey(option.Strike)) return;
                _options.Add(option.Strike, option);
                _optionsBySymbol.Add(option.Symbol, option);
            }
        }

        public double GetVol(DateTime asof, double forward, double strike)
        {
            var tenor = (SmileExpiry - asof).TotalDays * Constants.OneOver365;
            if (forward == 0 || strike == 0) return double.NaN;

            var ln = System.Math.Log(forward / strike);
            var one_minus_beta = 1 - Beta;
            var lnb = one_minus_beta * ln;
            var sigmaB = one_minus_beta * Alpha;
            var lnbSq = lnb * lnb;

            var mid = cf(forward, strike, Beta);
            var midSq = mid * mid;

            var f3 = sigmaB * sigmaB / (24.0 * midSq) + 0.25 * Rho * Alpha * Vega * Beta / mid + (2 - 3 * Rho * Rho) * Vega * Vega / 24.0;

            if (forward == strike) return Alpha / System.Math.Pow(forward, one_minus_beta) * (1 + f3 * tenor);

            var f1 = 1 + lnbSq / 24.0 + lnbSq * lnbSq / 1920.0;
            var f2 = Alpha / (mid * f1);
            var e = eta(Alpha, Vega, mid, forward, strike);

            return f2 * e / Xsi(Rho, e) * (1 + f3 * tenor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double cf(double f, double k, double Beta)
        {
            return System.Math.Pow(f * k, 0.5 * (1 - Beta));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double eta(double atm, double alpha, double cf, double f, double k)
        {
            return alpha / atm * cf * System.Math.Log(f / k);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Xsi(double rho, double eta)
        {
            var s = System.Math.Sqrt(1 - 2 * rho * eta + eta * eta);
            return System.Math.Log((s - rho + eta) / (1 - rho));
        }
    }
}
