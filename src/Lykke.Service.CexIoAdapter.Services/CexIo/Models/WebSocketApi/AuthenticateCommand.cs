using System.Security.Cryptography;
using System.Text;
using Lykke.Service.CexIoAdapter.Services.Tools;
using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi
{
    public sealed class AuthenticateCommand
    {
        private readonly long _unixTimestamp;
        private readonly string _apiKey;
        private readonly string _signature;

        public AuthenticateCommand(long unixTimestamp, string apiKey, string apiSecret)
        {
            _unixTimestamp = unixTimestamp;
            _apiKey = apiKey;
            _signature = CreateSignature(apiKey, unixTimestamp, apiSecret);
        }
        
        [JsonProperty("e")]
        public string Event => "auth";

        [JsonProperty("auth")]
        public object Content => new
        {
            key = _apiKey,
            timestamp = _unixTimestamp,
            signature = _signature
        };

        [JsonProperty("oid")]
        public string Oid => $"{_unixTimestamp}_auth";

        public static string CreateSignature(string apiKey, long unixTimestamp, string secretKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                return hmac.ComputeHash(Encoding.UTF8.GetBytes($"{unixTimestamp}{apiKey}")).Hex();
            }
        }

    }
}
