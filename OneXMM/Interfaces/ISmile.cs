using System;

namespace OneXMM.Interfaces
{
    using OneXMM.Pricing;

    public interface ISmile
    {
        DateTime SmileDate { get; }
        DateTime SmileExpiry { get; }
        double Expiry { get; }
        void UpdateSmileDate(DateTime smileDate);
        void UpdateParameters(params double[] args);
        void AddOption(Option option);
        double GetVol(double forward, double strike);
        double GetMoneyness(double forward, double strike);
        double GetVolFromMoneyness(double forward, double moneyness);
        double GetStrikeFromMoneyness(double forward, double moneyness);
    }
}
