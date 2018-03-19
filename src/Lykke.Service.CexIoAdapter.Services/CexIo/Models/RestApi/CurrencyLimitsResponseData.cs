using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public class CurrencyLimitsResponseData
    {
        [JsonProperty("pairs")]
        public CurrencyLimitInfo[] Pairs { get; set; }
    }
}