using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.Log;
using Lykke.Logs;
using Lykke.Sdk;
using Lykke.Service.CexIoAdapter.Services;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Services.Settings;
using Lykke.Service.CexIoAdapter.Settings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
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
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            var configurationRoot = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var settings = configurationRoot.LoadSettings<AppSettings>(options =>
            {
                options.SetConnString(x => x.SlackNotifications.AzureQueue.ConnectionString);
                options.SetQueueName(x => x.SlackNotifications.AzureQueue.QueueName);
                options.SenderName = $"{AppEnvironment.Name} {AppEnvironment.Version}";
            });

            services.AddLykkeLogging(
                settings.ConnectionString(x => x.CexIoAdapterService.Db.LogsConnString),
                "CexIoAdapterLog",
                settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                settings.CurrentValue.SlackNotifications.AzureQueue.QueueName);

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration(
                    "v1",
                    "CexIoAdapterService Test");
            });

            services.AddSingleton(settings.CurrentValue.CexIoAdapterService);

            //services.BuildServiceProvider<AppSettings>(options =>
            //{
            //    options.Logs = logs =>
            //    {
            //        logs.UseEmptyLogging();
            //    };

            //    options.Extend = (collection, manager) =>
            //    {
            //        collection.AddSingleton(manager.CurrentValue.CexIoAdapterService);
            //    };

            //    options.SwaggerOptions = new LykkeSwaggerOptions { ApiTitle = "CexIoAdapterService Test" };
            //});
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService<CexIoAdapterSettings>();
            var logFactory = app.ApplicationServices.GetService<ILogFactory>();

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
                        settings.OrderBooks.CurrencyMapping,
                        logFactory));
                    x.UseHandleBusinessExceptionsMiddleware();
                    x.UseMiddleware<CollectMetricsMiddleware>();
                };
            });
        }

        [UsedImplicitly]
        public void ConfigureTest(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            var settings = app.ApplicationServices.GetService<CexIoAdapterSettings>();
            var logFactory = app.ApplicationServices.GetService<ILogFactory>();

            XApiKeyAuthAttribute.Credentials =
                settings.Clients.ToDictionary(x => x.InternalApiKey, x => (object)x);

            app.UseLykkeConfiguration(options =>
            {
                options.WithMiddleware = x =>
                {
                    x.UseAuthenticationMiddleware(token => new CexIoRestClient(
                        GetCredentials(settings, token),
                        settings.OrderBooks.CurrencyMapping,
                        logFactory));
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
