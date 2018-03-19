using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Core.Domain.SharedContracts
{
    public sealed class WalletsResponse
    {
        [JsonProperty("wallets")]
        public IReadOnlyCollection<Wallet> Wallets { get; set; }
    }
}
