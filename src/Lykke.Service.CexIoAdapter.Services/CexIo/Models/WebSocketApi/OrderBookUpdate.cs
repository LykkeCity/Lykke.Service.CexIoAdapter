using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi
{
    public interface IOrderBookMessage : IMessageContent
    {
        string Pair { get; }
    }

    public sealed class OrderBookUpdate : IOrderBookMessage
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("pair")]
        public string Pair { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("bids")]
        public decimal[][] Bids { get; set; }

        [JsonProperty("asks")]
        public decimal[][] Asks { get; set; }
    }
}
