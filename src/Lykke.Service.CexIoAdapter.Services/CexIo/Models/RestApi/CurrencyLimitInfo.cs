using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public class CurrencyLimitInfo
    {
        [JsonProperty("symbol1")]
        public string Symbol1 { get; set; }

        [JsonProperty("symbol2")]
        public string Symbol2 { get; set; }

        [JsonProperty("minLotSize")]
        public decimal? MinLotSize { get; set; }

        [JsonProperty("minLotSizeS2")]
        public decimal? MinLotSizeS2 { get; set; }

        [JsonProperty("maxLotSize")]
        public decimal? MaxLotSize { get; set; }

        [JsonProperty("minPrice")]
        public string MinPrice { get; set; }

        [JsonProperty("maxPrice")]
        public string MaxPrice { get; set; }
    }
}
