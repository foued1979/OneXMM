using System;

namespace OneXMM
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using OneXMM.Attributes;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Blockchains
    {
        Error = 0,
        [BlockchainDetails(LiveUrl = "https://mainnet.infura.io/v3/641c3335cd904255b89bef32b537155f", TestUrl = "", BlockchainType = BlockchainTypes.EthereumLike)]
        Ethereum = 1,
        [BlockchainDetails(LiveUrl = "", TestUrl = "http://127.0.0.1:8545/", BlockchainType = BlockchainTypes.EthereumLike)]
        HardHat = 31337,
        [BlockchainDetails(LiveUrl = "", TestUrl = "", BlockchainType = BlockchainTypes.SolanaLike)]
        Solana = 1,
        [BlockchainDetails(LiveUrl = "", TestUrl = "", BlockchainType = BlockchainTypes.EthereumLike)]
        BSC = 56
    }
}
