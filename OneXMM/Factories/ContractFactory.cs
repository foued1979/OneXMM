using System;
using System.Reflection;
using Nethereum.ABI.Model;
using Nethereum.Contracts;
using Nethereum.Web3;

namespace OneXMM.Factories
{
    using OneXMM.Attributes;
    using OneXMM.Communication;
    using OneXMM.Interfaces;
    using OneXMM.Referential.Assets;
    using OneXMM.Resources.RawHardhatArtifact;
    using OneXMM.Utils;
    using System.Collections.Generic;

    public static class ContractFactory
    {
        private static Dictionary<Blockchains, Contract> _tokenContracts = new Dictionary<Blockchains, Contract>();
        private static SortedDictionary<Pair, Dictionary<KycStatus, Dictionary<PoolMaturities, Contract>>> _poolContracts = new SortedDictionary<Pair, Dictionary<KycStatus, Dictionary<PoolMaturities, Contract>>>();

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

        public static IRequestManager<TokenFunctions> CreateTokenRequestManager(Blockchains blockchain)
        {
            var attr = blockchain.GetAttributeForEnum<Blockchains, BlockchainDetailsAttribute>()!;

            if (attr.BlockchainType == BlockchainTypes.EthereumLike)
            {
                return new EthereumLikeRequestManager<TokenFunctions>(new EthereumAccount((int)blockchain), _tokenContracts[blockchain]);
            }

            throw new NotImplementedException();
        }

        public static Dictionary<PoolMaturities, IRequestManager<PoolFunctions>> CreatePoolRequestManager(Pair pair, KycStatus kycStatus)
        {
            var blockchain = pair.Chain;
            var attr = blockchain.GetAttributeForEnum<Blockchains, BlockchainDetailsAttribute>()!;

            if (attr.BlockchainType == BlockchainTypes.EthereumLike)
            {
                var res = new Dictionary<PoolMaturities, IRequestManager<PoolFunctions>>(Enum.GetValues<PoolMaturities>().Length);

                var contracts = _poolContracts[pair][kycStatus];
                
                foreach (var contract in contracts)
                {
                    var manager = new EthereumLikeRequestManager<PoolFunctions>(new EthereumAccount((int)blockchain), contract.Value);
                    res.Add(contract.Key, manager);
                }
                
                return res;
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
                            if (hardhatArtifact is null) throw new Exception($"ContractFactory - Cannot read artifact {embeddedResource}.");

                            switch (blockchain)
                            {
                                case Blockchains.Ethereum:
                                case Blockchains.HardHat:
                                    var contract = GetEthereumContract(blockchain, hardhatArtifact.ToString(), contractAddress, isTestMode);
                                    if (contract is null) throw new Exception($"ContractFactory - Token contract is null for blockchain {blockchain.ToString()} and address {contractAddress}.");
                                    _tokenContracts.Add(blockchain, contract);
                                    break;
                                default:
                                    throw new NotImplementedException($"GetSmartContracts - Unknown blockchain {blockchain}.");
                            }

                        }
                    }
                    else if (embeddedResource.Contains("Pool"))
                    {
                        var allPairs = Config.GetPairs();

                        foreach (var pair in allPairs) 
                        {
                            var contractAddressesForPair = Config.GetPoolContractAddresses(pair, isTestMode);
                            Dictionary<KycStatus, Dictionary<PoolMaturities, Contract>>? contractsByKycStatus = new Dictionary<KycStatus, Dictionary<PoolMaturities, Contract>>(Config.nKycStatus);

                            using (var stream = assembly.GetManifestResourceStream(embeddedResource))
                            using (var streamReader = new StreamReader(stream))
                            {
                                var abi = streamReader.ReadToEnd();
                                var hardhatArtifact = Utils.ConvertJsonToObject<HardhatArtifact>(abi);
                                if (hardhatArtifact is null) throw new Exception($"ContractFactory - Cannot read artifact {embeddedResource}.");

                                switch (blockchain)
                                {
                                    case Blockchains.Ethereum:
                                    case Blockchains.HardHat:
                                        foreach (var contractAddressesByKycStatus in contractAddressesForPair)
                                        {
                                            var contractsByMatu = new Dictionary<PoolMaturities, Contract>(Enum.GetValues<PoolMaturities>().Length);

                                            foreach (var matu in contractAddressesByKycStatus.Value)
                                            {
                                                var contract = GetEthereumContract(blockchain, hardhatArtifact.ToString(), matu.Value, isTestMode);
                                                if (contract is null) throw new Exception($"ContractFactory - Pool contract is null for blockchain {blockchain.ToString()} and address {matu.Value}.");

                                                contractsByMatu.Add(matu.Key, contract);
                                            }

                                            contractsByKycStatus.Add(contractAddressesByKycStatus.Key, contractsByMatu);
                                        }
                                        break;
                                    default:
                                        throw new NotImplementedException($"GetSmartContracts - Unknown blockchain {blockchain}.");
                                }
                            }
                        }
                    }
                }
            }
        }

        private static Contract? GetEthereumContract(Blockchains blockchain, string abi, string contractAddress, bool isTestMode)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("jsonrpc", "2.0");

            // The Infura token to communicate with the blockchain
            // When using Hardhat, this header is not important
            if (!isTestMode)
            {
                switch (Config.NodeInfo.Node)
                {
                    case Nodes.Infura:
                        httpClient.DefaultRequestHeaders.Add("user", Config.NodeInfo.ApiKey);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

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
