using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public class OrderBookSnapshot
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("bids")]
        public decimal[][] Bids { get; set; }

        [JsonProperty("asks")]
        public decimal[][] Asks { get; set; }

        [JsonProperty("pair")]
        public string Pair { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("sell_total")]
        public string SellTotal { get; set; }

        [JsonProperty("buy_total")]
        public string BuyTotal { get; set; }
    }
}
