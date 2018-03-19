using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi
{
    public sealed class PongCommand
    {
        [JsonProperty("e")]
        public string Event => Events.Pong;
    }
}
