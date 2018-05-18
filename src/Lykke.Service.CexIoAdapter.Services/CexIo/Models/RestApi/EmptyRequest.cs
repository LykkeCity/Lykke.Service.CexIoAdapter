using System.Security.Cryptography;
using System.Text;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Lykke.Service.CexIoAdapter.Services.Tools;
using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public class EmptyRequest
    {
        public EmptyRequest()
        {
        }

        public EmptyRequest(IApiCredentials credentials, long nonce)
        {
            Key = credentials.ApiKey;
            Nonce = nonce.ToString();
            Signature = CreateSignature(credentials.UserId, Nonce, credentials.ApiKey, credentials.ApiSecret);
        }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        public static string CreateSignature(string userId, string nonce, string apiKey, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var toSign = $"{nonce}{userId}{apiKey}";
                return hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign)).Hex(true);
            }
        }
    }
}
