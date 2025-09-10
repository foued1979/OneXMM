using System;

namespace OneXMM.Interfaces
{
    using OneXMM.Math.Optimizers.Constraints;
    using OneXMM.Pricing;

    public interface ISmile<T> : ISmile
        where T : ISmile<T>
    {
        static abstract ISmile operator -(T left, T right);
    }

    public interface ISmile
    {
        DateTime SmileDate { get; }
        DateTime SmileExpiry { get; }
        double ForwardRef { get; }
        double Expiry { get; }
        SmileConstraints GetSmileConstraints();
        void UpdateSmileDate(DateTime smileDate);
        void UpdateParameters(params double[] args);
        void AddOption(Option option);
        double GetVol(double forward, double strike);
        double GetMoneyness(double forward, double strike);
        double GetVolFromMoneyness(double forward, double moneyness);
        double GetStrikeFromMoneyness(double forward, double moneyness);
        double[] GetInitialGuess();
        ISmile Copy();

        ISmile GetForwardSmile(ISmile fromSmile);
    }
}
