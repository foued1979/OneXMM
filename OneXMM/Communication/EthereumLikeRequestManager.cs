using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using System;

namespace OneXMM.Communication
{
    internal sealed class EthereumLikeRequestManager<FType> : RequestManager<FType>
        where FType : Enum
    {
        private readonly Contract _contract;
        private readonly string _accountAddress;

        private SortedDictionary<FType, Function> _functionsMapping = new SortedDictionary<FType, Function>();
        private SortedDictionary<FType, bool> _functionHasOutput = new SortedDictionary<FType, bool>();

        public EthereumLikeRequestManager(Utils.EthereumAccount account, Contract contract) : base()
        {
            _contract = contract;
            _accountAddress = account.Account.Address;

            foreach (FType functionType in Enum.GetValues(typeof(FType)))
            {
                var functionBuilder = contract.ContractBuilder.GetFunctionBuilder(functionType.ToString());
                var function = contract.GetFunction(functionType.ToString());

                _functionHasOutput.Add(functionType, functionBuilder.FunctionABI.OutputParameters.Length > 0 && functionBuilder.FunctionABI.Constant == true);
                _functionsMapping.Add(functionType, function);
            }
        }

        public override dynamic ExecuteTask(FType functionType, params object[] parameters)
        {
            if (_queueMapping[functionType] && _functionHasOutput[functionType])
            {
                while (_queueMapping[functionType]) ;
                return _lastResult[functionType];
            }
            else
            {
                _queueMapping[functionType] = true;

                
                    var function = _functionsMapping[functionType];

                if (_functionHasOutput[functionType])
                {
                    try
                    {
#if DEBUG
                        var response = function.CallAsync(function.CreateCallInput(parameters)).ConfigureAwait(false).GetAwaiter().GetResult();
#endif
                        var res = function.CallDecodingToDefaultAsync(parameters).ConfigureAwait(false).GetAwaiter().GetResult();

                        _lastResult[functionType] = res;
                        return res;
                    }
                    catch
                    {
                        return _lastResult[functionType];
                    }
                    finally
                    {
                        _queueMapping[functionType] = false;
                    }
                }
                else
                {
                    try
                    {
                        var gasEstimate = function.EstimateGasAsync(parameters).GetAwaiter().GetResult();
                        var value = new HexBigInteger("0x0");

                        return function.SendTransactionAndWaitForReceiptAsync(_accountAddress, gasEstimate, value, new CancellationToken(), parameters).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Transaction error", ex);
                    }
                }
                
            }
        }
    }
}
