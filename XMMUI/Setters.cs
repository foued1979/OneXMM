using OneXMM.Attributes;
using OneXMM.Factories;
using OneXMM.Interfaces;
using OneXMM.Utils;
using OneXMM;
using System;

namespace XMMUI
{
    public partial class OneXMMUI
    {
        private static IRequestManager<TokenFunctions> _tokenRequestManager;

        private void Set()
        {
            if (_settings.IsTestMode) { hardhatToolStripMenuItem.Checked = true; _selectedBlockchain = Blockchains.HardHat; }
            else { ethereumToolStripMenuItem.Checked = true; _selectedBlockchain = Blockchains.Ethereum; }

            SetRequestManager();
        }

        private void SetRequestManager()
        {
            var attr = _selectedBlockchain.GetAttributeForEnum<Blockchains, BlockchainDetailsAttribute>();

            if (attr.BlockchainType == BlockchainTypes.EthereumLike)
            {
                _tokenRequestManager = ContractFactory.CreateRequestManager<TokenFunctions>(_selectedBlockchain);
            }
            else
            {
                throw new Exception("Non Ethereum-like blockchains not managed.");
            }
        }
    }
}
