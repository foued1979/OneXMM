using System;

namespace OneXMM.Pricing
{
    using MathNet.Numerics.Integration;
    using OneXMM.Referential;
    using System.Runtime.CompilerServices;

    public sealed class SABR
    {
        private double tenor;
        private double fmid;
        private double g1;
        private double g2;

        public DateTime SmileDate { get; }
        public DateTime SmileExpiry { get; }
        
        public double Alpha { get; private set; }
        public double Vega { get; private set; }
        public double Beta { get; private set; }
        public double Rho { get; private set; }

        public SortedDictionary<string, Option> _optionsBySymbol { get; private set; } = new SortedDictionary<string, Option>();
        public SortedDictionary<double, Option> Options { get; private set; } = new SortedDictionary<double, Option>();

        public SABR(DateTime smileDate, DateTime smileExpiry)
        {
            SmileDate = smileDate;
            SmileExpiry = smileExpiry;

            tenor = (smileExpiry - smileDate).TotalDays * Constants.OneOver365;
        }

        public SABR WithParameters(double alpha, double vega, double beta, double rho)
        {
            Alpha = alpha;
            Vega = vega;
            Beta = beta;
            Rho = rho;

            return this;
        }

        public void AddOption(Option option)
        {
            if (option.Expiry.CompareTo(SmileExpiry) != 0) throw new Exception($"Wrong option attached to SABR smile for {option.Pair.Symbol}.");

            lock (Options)
            {
                if (Options.ContainsKey(option.Strike)) return;
                Options.Add(option.Strike, option);
                _optionsBySymbol.Add(option.Symbol, option);
            }
        }

        public double GetVol(double forward, double strike)
        {
            if (forward == 0 || strike == 0) return double.NaN;

            var ln = Math.Log(forward / strike);
            var one_minus_beta = 1 - Beta;
            var lnb = one_minus_beta * ln;
            var sigmaB = one_minus_beta * Alpha;
            var lnbSq = lnb * lnb;

            var mid = cf(forward, strike, Beta);
            var midSq = mid * mid;

            var f3 = sigmaB * sigmaB / (24.0 * midSq) + 0.25 * Rho * Alpha * Vega * Beta / mid + (2 - 3 * Rho * Rho) * Vega * Vega / 24.0;

            if (forward == strike) return Alpha / Math.Pow(forward, one_minus_beta) * (1 + f3 * tenor);

            var f1 = 1 + lnbSq / 24.0 + lnbSq * lnbSq / 1920.0;
            var f2 = Alpha / (mid * f1);
            var e = eta(Alpha, Vega, mid, forward, strike);

            return f2 * e / Xsi(Rho, e) * (1 + f3 * tenor);
        }
        // alpha is sigma0
        // Vega is alpha
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double cf(double f, double k, double Beta)
        {
            return Math.Pow(f * k, 0.5 * (1 - Beta));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double eta(double atm, double alpha, double cf, double f, double k)
        {
            return alpha / atm * cf * Math.Log(f / k);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Xsi(double rho, double eta)
        {
            var s = Math.Sqrt(1 - 2 * rho * eta + eta * eta);
            return Math.Log((s - rho + eta) / (1 - rho));
        }
    }
}
