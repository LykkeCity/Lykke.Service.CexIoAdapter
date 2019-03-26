using System.IO;
using System.Linq;
using System.Net.Http;
using Lykke.Service.CexIoAdapter.Services.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;

namespace Lykke.Service.CexIoAdapter.Tests
{
    public class CexioIntegrationWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private readonly LaunchSettingsFixture _launchSettingsFixture;

        private const string ApiKeyHeader = "X-API-KEY";

        public CexioIntegrationWebApplicationFactory()
        {
            _launchSettingsFixture = new LaunchSettingsFixture();
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);

            var settings = Server.Host.Services.GetRequiredService<CexIoAdapterSettings>();

            var credentials = settings.Clients.RandomSubset(1).Single();

            client.DefaultRequestHeaders.Add(ApiKeyHeader, credentials.InternalApiKey);
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:5000")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseApplicationInsights();

            return builder;
        }
    }
}
