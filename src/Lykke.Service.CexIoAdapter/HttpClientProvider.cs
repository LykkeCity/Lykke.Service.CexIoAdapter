using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.CexIoAdapter
{
    internal static class HttpClientProvider
    {
        private static readonly IHttpClientFactory ClientFactory;

        static HttpClientProvider()
        {
            ClientFactory = new ServiceCollection()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetRequiredService<IHttpClientFactory>();
        }


        public static HttpClient Client
            => ClientFactory.CreateClient();
    }
}
