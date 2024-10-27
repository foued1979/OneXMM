using System;
using System.Reflection;

namespace OneXMM.Factories
{
    using Nethereum.Contracts;
    using Nethereum.Web3;
    using OneXMM.Attributes;
    using OneXMM.Communication;
    using OneXMM.Interfaces;
    using OneXMM.Resources.RawHardhatArtifact;
    using OneXMM.Utils;

    public static class ContractFactory
    {
        public static Contract[] GetEthereumLikeContracts(Blockchains blockchain, string contractAddress, bool isTestMode)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var embeddedResources = assembly.GetManifestResourceNames();

            var contracts = new List<Contract>(10);

            foreach (var embeddedResource in embeddedResources)
            {
                if (embeddedResource.Contains("OneXMM.Resources") && embeddedResource.EndsWith(".json"))
                {
                    using (var stream = assembly.GetManifestResourceStream(embeddedResource))
                    using (var streamReader = new StreamReader(stream))
                    {
                        var abi = streamReader.ReadToEnd();
                        var hardhatArtifact = Utils.ConvertJsonToObject<HardhatArtifact>(abi);
                        if (hardhatArtifact is null) continue;

                        switch (blockchain)
                        {
                            case Blockchains.Ethereum:
                            case Blockchains.HardHat:
                                var contract = GetEthereumContract(hardhatArtifact.ToString(), contractAddress, isTestMode);
                                if (contract is not null) contracts.Add(contract);
                                break;
                            default:
                                throw new NotImplementedException($"GetSmartContracts - Unknown blockchain {blockchain}.");
                        }
                        
                    }
                }
            }

            return contracts.ToArray();
        }

        public static IRequestManager<FType> CreateRequestManager<FType>(object contract, bool isTestMode)
            where FType : Enum
        {
            if (contract is Contract ethContract)
            {
                return new EthereumLikeRequestManager<FType>(new EthereumAccount(isTestMode ? (int)Blockchains.HardHat : (int)Blockchains.Ethereum), ethContract);
            }

            throw new NotImplementedException();
        }

        private static Contract? GetEthereumContract(string abi, string contractAddress, bool isTestMode)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("jsonrpc", "2.0");

            // The Infura token to communicate with the blockchain
            // When using Hardhat, this header is not important
            if (!isTestMode) httpClient.DefaultRequestHeaders.Add("user", "7cb2a61ab7d84b6a995f7ec68ac2bc89");

            var attr = isTestMode
                ? Blockchains.HardHat.GetAttributeForEnum<Blockchains, BlockchainDetailsAttribute>()
                : Blockchains.Ethereum.GetAttributeForEnum<Blockchains, BlockchainDetailsAttribute>();

            if (attr is null) return null;

            var uri = isTestMode ? attr.TestUrl : attr.LiveUrl;

            var rpc = new Nethereum.JsonRpc.Client.RpcClient(new Uri(uri), httpClient);
            var web3 = new Web3(new EthereumAccount((int)Blockchains.Ethereum).Account, rpc);
            return web3.Eth.GetContract(abi, contractAddress);
        }
    }
}
