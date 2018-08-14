using Lykke.Service.CexIoAdapter.Services.Settings;
using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public sealed class GetOrderRequest : EmptyRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public GetOrderRequest(string id, IApiCredentials credentials, long nonce)
            : base(credentials, nonce)
        {
            Id = id;
        }
    }
}
