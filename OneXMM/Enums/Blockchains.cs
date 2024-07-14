using System;

namespace OneXMM
{
    using OneXMM.Attributes;

    public enum Blockchains
    {
        [BlockchainDetails(LiveUrl = "https://mainnet.infura.io/v3/641c3335cd904255b89bef32b537155f", TestUrl = "", BlockchainType = BlockchainTypes.EthereumLike)]
        Ethereum = 1,
        [BlockchainDetails(LiveUrl = "", TestUrl = "http://127.0.0.1:8545/", BlockchainType = BlockchainTypes.EthereumLike)]
        HardHat = 31337
    }
}
