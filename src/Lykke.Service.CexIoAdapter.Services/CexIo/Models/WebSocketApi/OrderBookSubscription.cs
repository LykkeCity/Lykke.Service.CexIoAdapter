using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi
{
    public class OrderBookUnsubscribed : IMessageContent
    {
        [JsonProperty("pair")]
        public string Pair { get; set; }
    }


    public sealed class OrderBookSubscription : IOrderBookMessage
    {
        private IReadOnlyCollection<decimal[]> _asks;
        private IReadOnlyCollection<decimal[]> _bids;

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("bids")]
        public IReadOnlyCollection<decimal[]> Bids
        {
            get => _bids ?? new decimal[][]{};
            set => _bids = value;
        }

        [JsonProperty("asks")]
        public IReadOnlyCollection<decimal[]> Asks
        {
            get => _asks ?? new decimal[][]{};
            set => _asks = value;
        }

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
