using System;

namespace OneXMM.Services
{
    using OneXMM.Factories;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;

    public static class PoolRequestManagerServices
    {
        private static SortedDictionary<Pair, Dictionary<PoolMaturities, IRequestManager<PoolFunctions>>> _poolRequestManagers = new SortedDictionary<Pair, Dictionary<PoolMaturities, IRequestManager<PoolFunctions>>>();

        public static void AddPair(Pair pair)
        {
            if (_poolRequestManagers.ContainsKey(pair)) return;

            var requestManagers = new Dictionary<PoolMaturities, IRequestManager<PoolFunctions>>(Enum.GetValues<PoolMaturities>().Length);

            var maturitiesForPair = Config.GetPoolMaturities(pair);

            foreach (var poolMaturity in maturitiesForPair)
                requestManagers.Add(poolMaturity, ContractFactory.CreateRequestManager<PoolFunctions>(pair.Chain));

            _poolRequestManagers.Add(pair, requestManagers);
        }

        public static bool UpdateFirstFixing(Pair pair, PoolMaturities poolMaturity, double spot, double strike, double volatility, double premium)
        {
            var requestManager = _poolRequestManagers[pair][poolMaturity] ?? throw new Exception($"PoolRequestManagerServices - Request manager not set for pair {pair.Symbol} and maturity {poolMaturity.ToString()}.");
            try
            {
                var tx = requestManager.ExecuteTask(PoolFunctions.setFirstFixing, [
                    spot * Constants.One_32,
                    strike * Constants.One_32,
                    volatility * Constants.One_32,
                    premium * Constants.One_32]);

                var lastRateIndices = requestManager.ExecuteTask(PoolFunctions.getAllLastRateIndices);
                throw new NotImplementedException();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool UpdateFixing(Pair pair, PoolMaturities poolMaturity, double spot, double strike, double volatility, double premium)
        {
            var requestManager = _poolRequestManagers[pair][poolMaturity] ?? throw new Exception($"PoolRequestManagerServices - Request manager not set for pair {pair.Symbol} and maturity {poolMaturity.ToString()}.");
            try
            {
                var tx = requestManager.ExecuteTask(PoolFunctions.setFixing, [
                    spot * Constants.One_32,
                    strike * Constants.One_32,
                    volatility * Constants.One_32,
                    premium * Constants.One_32]);

                var lastRateIndices = requestManager.ExecuteTask(PoolFunctions.getAllLastRateIndices);
                throw new NotImplementedException();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
