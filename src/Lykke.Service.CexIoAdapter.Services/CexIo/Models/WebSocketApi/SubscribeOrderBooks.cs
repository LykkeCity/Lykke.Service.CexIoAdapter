using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi
{
    public sealed class SubscribeOrderBooksCommand
    {
        private readonly string _from;
        private readonly string _to;

        public SubscribeOrderBooksCommand(string from, string to)
        {
            _from = from;
            _to = to;
        }

        [JsonProperty("e")]
        public string Event => Events.OrderBookSubscribe;

        [JsonProperty("data")]
        public object Data => new
        {
            pair = new[] {_from, _to},
            subscribe = true,
            depth = 0
        };
    }
}
