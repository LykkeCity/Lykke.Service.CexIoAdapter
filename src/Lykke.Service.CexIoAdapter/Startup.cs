using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.Log;
using Lykke.Sdk;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Services.Settings;
using Lykke.Service.CexIoAdapter.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.CexIoAdapter
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "CexIoAdapterService"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Logs = logs =>
                {
                    logs.AzureTableName = "CexIoAdapterLog";
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.CexIoAdapterService.Db.LogsConnString;
                };

                options.Swagger = ClientTokenMiddleware.ConfigureSwagger;
                options.SwaggerOptions = _swaggerOptions;
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService<CexIoAdapterSettings>();
            var logFactory = app.ApplicationServices.GetService<ILogFactory>();

            XApiKeyAuthAttribute.Credentials =
                settings.Clients.ToDictionary(x => x.InternalApiKey, x => (object) x);

            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.WithMiddleware = x =>
                {
                    x.UseAuthenticationMiddleware(token => new CexIoRestClient(
                        GetCredentials(settings, token),
                        settings.OrderBooks.CurrencyMapping));
                    x.UseHandleBusinessExceptionsMiddleware();
                };
            });
        }

        private ApiCredentials GetCredentials(CexIoAdapterSettings settings, string token)
        {
            var s = settings.Clients.FirstOrDefault(x => x.InternalApiKey == token);

            if (s == null) return null;

            return new ApiCredentials
            {
                ApiKey = s.ApiKey,
                ApiSecret = s.ApiSecret,
                UserId = s.UserId
            };
        }
    }
}
