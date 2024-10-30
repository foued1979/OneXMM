using System;
using System.Reflection;

namespace OneXMM.Factories
{
    using Nethereum.ABI.Model;
    using Nethereum.Contracts;
    using Nethereum.Web3;
    using OneXMM.Attributes;
    using OneXMM.Communication;
    using OneXMM.Interfaces;
    using OneXMM.Resources.RawHardhatArtifact;
    using OneXMM.Utils;

    public static class ContractFactory
    {
        private static Dictionary<Blockchains, Contract> _tokenContracts = new Dictionary<Blockchains, Contract>();

        public static void SetContractFactory(bool isTestMode)
        {
            if (isTestMode) SetEthereumLikeContracts(Blockchains.HardHat, isTestMode);
#if !DEBUG
            _contracts.Add(Blockchains.Ethereum, GetEthereumLikeContracts(Blockchains.Ethereum, isTestMode));
#endif
        }

        public static FunctionABI[] GetTokenContractFunctions(Blockchains blockchain)
        {
            return _tokenContracts[blockchain].ContractBuilder.ContractABI.Functions;
        }

        public static IRequestManager<FType> CreateRequestManager<FType>(Blockchains blockchain)
            where FType : Enum
        {
            var attr = blockchain.GetAttributeForEnum<Blockchains, BlockchainDetailsAttribute>()!;

            if (attr.BlockchainType == BlockchainTypes.EthereumLike)
            {
                return new EthereumLikeRequestManager<FType>(new EthereumAccount((int)blockchain), GetContract<FType>(blockchain));
            }

            throw new NotImplementedException();
        }

        private static void SetEthereumLikeContracts(Blockchains blockchain, bool isTestMode)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var embeddedResources = assembly.GetManifestResourceNames();

            foreach (var embeddedResource in embeddedResources)
            {
                if (embeddedResource.Contains("OneXMM.Resources") && embeddedResource.EndsWith(".json"))
                {
                    if (embeddedResource.Contains("Token"))
                    {
                        var contractAddress = Config.GetTokenContractAddress(blockchain, isTestMode);

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
                                    var contract = GetEthereumContract(blockchain, hardhatArtifact.ToString(), contractAddress, isTestMode);
                                    if (contract is null) throw new Exception($"ContractFactory - Contrac is null for blockchain {blockchain.ToString()} and address {contractAddress}.");
                                    _tokenContracts.Add(blockchain, contract);
                                    break;
                                default:
                                    throw new NotImplementedException($"GetSmartContracts - Unknown blockchain {blockchain}.");
                            }

                        }
                    }
                    else if (embeddedResource.Contains("Pool"))
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        private static Contract GetContract<FType>(Blockchains blockchain) where FType : Enum
        {
            if (typeof(FType).Equals(typeof(TokenFunctions)))
            {
                return _tokenContracts[blockchain];
            }

            throw new NotImplementedException();
        }

        private static Contract? GetEthereumContract(Blockchains blockchain, string abi, string contractAddress, bool isTestMode)
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
            var web3 = new Web3(new EthereumAccount((int)blockchain).Account, rpc);
            return web3.Eth.GetContract(abi, contractAddress);
        }
    }
}
