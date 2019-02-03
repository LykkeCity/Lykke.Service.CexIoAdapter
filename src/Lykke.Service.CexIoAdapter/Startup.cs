using System;
using System.Linq;
using System.Reflection;
using Autofac;
using JetBrains.Annotations;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Sdk;
using Lykke.Service.CexIoAdapter.Modules;
using Lykke.Service.CexIoAdapter.Services;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Services.Settings;
using Lykke.Service.CexIoAdapter.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Nexogen.Libraries.Metrics.Prometheus.AspCore;

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
            services.AddPrometheus(Metrics.Prometheus);
            services.AddSingleton(new HttpMetrics(Metrics.Prometheus));

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
        public void ConfigureTestServices(IServiceCollection services)
        {
            services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Logs = logs =>
                {
                    logs.UseEmptyLogging();
                };

                options.Extend = (collection, manager) =>
                {
                    collection.AddSingleton(manager.CurrentValue.CexIoAdapterService);
                };

                options.SwaggerOptions = new LykkeSwaggerOptions { ApiTitle = "CexIoAdapterService Test" };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService<CexIoAdapterSettings>();
            // var logFactory = app.ApplicationServices.GetService<ILogFactory>();

            XApiKeyAuthAttribute.Credentials =
                settings.Clients.ToDictionary(x => x.InternalApiKey, x => (object) x);

//            app.UsePrometheus(options =>
//            {
//                options.CollectHttpMetrics();
//            });

            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.WithMiddleware = x =>
                {
                    x.UseAuthenticationMiddleware(token => new CexIoRestClient(
                        GetCredentials(settings, token),
                        settings.OrderBooks.CurrencyMapping));
                    x.UseHandleBusinessExceptionsMiddleware();
                    x.UseMiddleware<CollectMetricsMiddleware>();
                };
            });
        }

        [UsedImplicitly]
        public void ConfigureTest(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            var settings = app.ApplicationServices.GetService<CexIoAdapterSettings>();

            XApiKeyAuthAttribute.Credentials =
                settings.Clients.ToDictionary(x => x.InternalApiKey, x => (object)x);

            app.UseLykkeConfiguration(options =>
            {
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
