using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi
{
    public sealed class CexIoResponse
    {
        [JsonProperty("e")]
        public string Event { get; set; }

        [JsonProperty("oid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Oid { get; set; }

        [JsonProperty("time")]
        public long? Time { get; set; }

        [JsonProperty("data")]
        public JToken Json { get; set; }

        [JsonIgnore]
        public IMessageContent Message { get; internal set; }

        [JsonProperty("ok", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Ok { get; set; }
    }
}

