using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public class Wallet
    {
        [JsonProperty("asset")]
        public string Asset { get; set; }
        [JsonProperty("balance")]
        public decimal Balance { get; set; }
        [JsonProperty("reserved")]
        public decimal Reserved { get; set; }
    }
}
