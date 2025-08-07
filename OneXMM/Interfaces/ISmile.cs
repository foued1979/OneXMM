using System;

namespace OneXMM.Interfaces
{
    using OneXMM.Referential;

    public interface ISmile
    {
        DateTime SmileDate { get; }
        DateTime SmileExpiry { get; }
        void UpdateSmileDate(DateTime smileDate);
        void UpdateParameters(params double[] args);
        void AddOption(Option option);
        double GetVol(DateTime asof, double forward, double strike);
    }
}
