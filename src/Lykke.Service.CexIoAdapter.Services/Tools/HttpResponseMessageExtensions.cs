using System;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.CexIoAdapter.Services.Tools
{
    public static class HttpResponseMessageExtensions
    {
        public static HttpResponseMessage EnsureSuccessContent(
            this HttpResponseMessage response,
            CancellationToken ct = default(CancellationToken))
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            // todo: consider JArray and JValue also
            var obj = response.Content.ReadAsAsync<JObject>(ct).Result;

            if (obj["error"] != null)
            {
                throw new Exception($"Error in response: {obj["error"]}");
            }

            return response;
        }
    }
}
