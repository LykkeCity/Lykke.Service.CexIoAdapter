using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public sealed class Balance
    {
        [JsonProperty("available")]
        public decimal Available { get; set; }

        [JsonProperty("orders")]
        public decimal? Orders { get; set; }
    }
}