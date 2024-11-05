using System;

namespace OneXMM.Communication.TaskResults
{
    public struct SetFixingResult
    {
        public bool KYCPoolError { get; }
        public bool NonKCPoolError { get; }

        public SetFixingResult(bool kycError, bool nonkycError)
        {
            KYCPoolError = kycError;
            NonKCPoolError = nonkycError;
        }
    }
}
