using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public class CurrencyLimitsResponse
    {
        [JsonProperty("e")]
        public string Event { get; set; }

        [JsonProperty("ok")]
        public string Ok { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("data")]
        public CurrencyLimitsResponseData Data { get; set; }
    }
}