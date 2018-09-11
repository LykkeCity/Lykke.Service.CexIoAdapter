using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public sealed class WalletsResponse
    {
        [JsonProperty("wallets")]
        public IReadOnlyCollection<Wallet> Wallets { get; set; }
    }
}
