using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public sealed class Order
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("instrument")]
        public string Instrument { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("volume")]
        public decimal Volume { get; set; }

        [JsonProperty("orderType")]
        public string OrderType { get; set; }

        [JsonProperty("tradeType")]
        public string TradeType { get; set; }
    }
}
