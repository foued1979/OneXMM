using System;

namespace OneXMM.Services
{
    using OneXMM.Communication.TaskResults;
    using OneXMM.Factories;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;

    public static class PoolRequestManagerServices
    {
        private static SortedDictionary<Pair, Dictionary<PoolMaturities, IRequestManager<PoolFunctions>>> _nonkycPoolRequestManagers = new SortedDictionary<Pair, Dictionary<PoolMaturities, IRequestManager<PoolFunctions>>>();
        private static SortedDictionary<Pair, Dictionary<PoolMaturities, IRequestManager<PoolFunctions>>> _kycPoolRequestManagers = new SortedDictionary<Pair, Dictionary<PoolMaturities, IRequestManager<PoolFunctions>>>();

        public static void AddPair(Pair pair)
        {
            var maturitiesForPair = Config.GetPoolMaturities(pair);

            // We check first the Non-Kyc
            if (!_nonkycPoolRequestManagers.ContainsKey(pair))
            {
                var requestManagers = ContractFactory.CreatePoolRequestManager(pair, KycStatus.NonKYC);
                _nonkycPoolRequestManagers.Add(pair, requestManagers);
            }

            // Then we check first the Kyc
            if (!_kycPoolRequestManagers.ContainsKey(pair))
            {
                var requestManagers = ContractFactory.CreatePoolRequestManager(pair, KycStatus.KYC);
                _kycPoolRequestManagers.Add(pair, requestManagers);
            }
        }

        public static SetFixingResult UpdateFirstFixing(Pair pair, PoolMaturities poolMaturity, double spot, double strike, double volatility, double premium)
        {
            bool nonKycSuccess = true;
            bool kycSuccess = true;

            if (_nonkycPoolRequestManagers[pair].TryGetValue(poolMaturity, out var nonKycRequestManager))
            {
                nonKycSuccess = Task.Run(() => UpdateFixingTask(PoolFunctions.setFirstFixing, spot, strike, volatility, premium, nonKycRequestManager)).ConfigureAwait(false).GetAwaiter().GetResult();

                if (nonKycSuccess)
                {
                    throw new NotImplementedException("Must create a task which will get all the updates, and store info in database.");
                    var lastRateIndices = nonKycRequestManager.ExecuteTask(PoolFunctions.getAllLastRateIndices);
                }
            }

            if (_kycPoolRequestManagers[pair].TryGetValue(poolMaturity, out var kycRequestManager))
            {
                kycSuccess = Task.Run(() => UpdateFixingTask(PoolFunctions.setFirstFixing, spot, strike, volatility, premium, kycRequestManager)).ConfigureAwait(false).GetAwaiter().GetResult();

                if (kycSuccess)
                {
                    throw new NotImplementedException("Must create a task which will get all the updates, and store info in database.");
                    var lastRateIndices = kycRequestManager.ExecuteTask(PoolFunctions.getAllLastRateIndices);
                }
            }

            return new SetFixingResult(kycSuccess, nonKycSuccess);
        }

        public static SetFixingResult UpdateFixing(Pair pair, PoolMaturities poolMaturity, double spot, double strike, double volatility, double premium)
        {
            bool nonKycSuccess = true;
            bool kycSuccess = true;

            if (_nonkycPoolRequestManagers[pair].TryGetValue(poolMaturity, out var nonKycRequestManager))
            {
                nonKycSuccess = Task.Run(() => UpdateFixingTask(PoolFunctions.setFixing, spot, strike, volatility, premium, nonKycRequestManager)).ConfigureAwait(false).GetAwaiter().GetResult();

                if (nonKycSuccess)
                {
                    throw new NotImplementedException("Must create a task which will get all the updates, and store info in database.");
                    var lastRateIndices = nonKycRequestManager.ExecuteTask(PoolFunctions.getAllLastRateIndices);
                }
            }

            if (_kycPoolRequestManagers[pair].TryGetValue(poolMaturity, out var kycRequestManager))
            {
                kycSuccess = Task.Run(() => UpdateFixingTask(PoolFunctions.setFixing, spot, strike, volatility, premium, kycRequestManager)).ConfigureAwait(false).GetAwaiter().GetResult();

                if (kycSuccess)
                {
                    throw new NotImplementedException("Must create a task which will get all the updates, and store info in database.");
                    var lastRateIndices = kycRequestManager.ExecuteTask(PoolFunctions.getAllLastRateIndices);
                }
            }

            return new SetFixingResult(kycSuccess, nonKycSuccess);
        }

        private static Func<PoolFunctions, double, double, double, double, IRequestManager<PoolFunctions>, bool> UpdateFixingTask = (func, spot, strike, volatility, premium, requestManager) =>
        {
            try
            {
                var tx = requestManager.ExecuteTask(func, [
                    spot * Constants.One_32,
                    strike * Constants.One_32,
                    volatility * Constants.One_32,
                    premium * Constants.One_32]);

                return true;
            }
            catch
            {
                return false;
            }
        };
    }
}
