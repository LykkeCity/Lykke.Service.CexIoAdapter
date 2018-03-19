using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Service.CexIoAdapter.Services.Tools
{
    public static class HttpClientExtensions
    {
        public static async Task<T> GetAsAsync<T>(
            this HttpClient client,
            string requestUri,
            CancellationToken ct)
        {
            using (var response = await client.GetAsync(requestUri, ct))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<T>(ct);
            }
        }
    }
}
