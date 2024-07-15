using System;

namespace OneXMM.Pricing
{
    using OneXMM.Referential;

    public sealed class SABR
    {
        private double tenor;
        private double e;
        private double fmid;
        private double g1;
        private double g2;

        public DateTime SmileDate { get; }
        public DateTime SmileExpiry { get; }
        
        public double ATMVol { get; private set; }
        public double Alpha { get; private set; }
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

        public SABR WithParameters(double atm, double alpha, double beta, double rho)
        {
            e = tenor * alpha * alpha;
            ATMVol = atm;
            Alpha = alpha;
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

            fmid = System.Math.Sqrt(forward * strike);
            var cf = System.Math.Pow(fmid, Beta);

            g1 = Beta / fmid;
            g2 = -Beta * (1 - Beta) / (fmid * fmid);

            var eta = Alpha / (ATMVol * (1 - Beta)) * (System.Math.Pow(forward, 1 - Beta) - System.Math.Pow(strike, 1 - Beta));
            var s = System.Math.Sqrt(1 - 2 * Rho * eta + eta * eta);
            var D = System.Math.Log((s + eta - Rho) / (1 - Rho));

            var fact = ATMVol * cf / Alpha;
            var f1 = (2 * g2 - g1 * g1 + 1.0 / (fmid * fmid)) / 24.0 * fact * fact;
            var f2 = Rho * g1 / 4.0 * fact;
            var f3 = (2.0 - 3 * Rho * Rho) / 24.0;

            return Alpha * System.Math.Log(forward / strike) / D * (1 + e * (f1 + f2 + f3));
        }
    }
}
