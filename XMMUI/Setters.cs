using OneXMM.Attributes;
using OneXMM.Factories;
using OneXMM.Interfaces;
using OneXMM.Utils;
using OneXMM;
using System;
using Nethereum.ABI.Model;
using System.Runtime.CompilerServices;

namespace XMMUI
{
    public partial class OneXMMUI
    {
        private static IRequestManager<TokenFunctions> _tokenRequestManager;
        private static SortedDictionary<string, Parameter[]> _tokenFunctionParameters;
        private static SortedDictionary<string, Parameter[]> _tokenFunctionOutputs;

        private void Set()
        {
            if (_settings.IsTestMode) { hardhatToolStripMenuItem.Checked = true; _selectedBlockchain = Blockchains.HardHat; }
            else { ethereumToolStripMenuItem.Checked = true; _selectedBlockchain = Blockchains.Ethereum; }

            foreach (TokenFunctions function in Enum.GetValues(typeof(TokenFunctions)))
            {
                tokenFunctionList.Items.Add(function.ToString());
            }

            SetRequestManager();
        }

        private void SetRequestManager()
        {
            var attr = _selectedBlockchain.GetAttributeForEnum<Blockchains, BlockchainDetailsAttribute>();

            if (attr.BlockchainType == BlockchainTypes.EthereumLike)
            {
                _tokenRequestManager = ContractFactory.CreateRequestManager<TokenFunctions>(_selectedBlockchain);
                _tokenFunctionParameters = new SortedDictionary<string, Parameter[]>();
                _tokenFunctionOutputs = new SortedDictionary<string, Parameter[]>();

                var functions = ContractFactory.GetTokenContractFunctions(_selectedBlockchain);

                foreach (var function in functions)
                {
                    var cleanName = CleanSignatureName(function.Signature);
                    _tokenFunctionParameters.Add(cleanName, function.InputParameters);
                    if (function.Constant) _tokenFunctionOutputs.Add(cleanName, function.OutputParameters);
                }    
            }
            else
            {
                throw new Exception("Non Ethereum-like blockchains not managed.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string CleanSignatureName(string signature)
        {
            return signature.Substring(0, signature.IndexOf('('));
        }
    }
}
