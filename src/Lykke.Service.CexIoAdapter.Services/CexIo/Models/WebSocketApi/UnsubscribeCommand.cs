using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi
{
    public class UnsubscribeCommand
    {
        private readonly string _from;
        private readonly string _to;

        public UnsubscribeCommand(string from, string to)
        {
            _from = from;
            _to = to;
        }

        [JsonProperty("e")]
        public string Event => Events.OrderBookUnsubscribe;

        [JsonProperty("data")]
        public object Data =>
            new
            {
                pair = new[] {_from, _to}
            };

        [JsonProperty("oid")]
        public string Oid { get; set; }
    }
}
