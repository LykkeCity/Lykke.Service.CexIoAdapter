using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi
{
    public class AuthenticationResult : IMessageContent
    {
        [JsonProperty("ok")]
        public string Ok { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
