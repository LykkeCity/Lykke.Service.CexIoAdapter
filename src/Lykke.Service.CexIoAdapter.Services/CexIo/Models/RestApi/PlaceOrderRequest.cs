using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public sealed class PlaceOrderRequest : EmptyRequest
    {
        public PlaceOrderRequest(IApiCredentials creds, long nonce) : base(creds, nonce)
        {
        }

        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public OrderType OrderType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }
    }
}
