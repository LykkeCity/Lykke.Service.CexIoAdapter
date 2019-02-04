using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using JetBrains.Annotations;
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
using JsonConvert = Newtonsoft.Json.JsonConvert;

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

            // @atarutin: Workaround to load settings manually since there are troubles with latest
            // Lykke.SettingsReader library version and HttpClient instance initialization when
            // executing tests with TeamCity
            var settingsUrl = configurationRoot[SettingsConfiguratorExtensions.DefaultConfigurationKey];

            if (string.IsNullOrEmpty(settingsUrl))
                throw new InvalidOperationException("SettingsUrl variable is empty");

            string settingsContent;

            if (settingsUrl.StartsWith("http"))
            {
                using (var httpClient = new HttpClient())
                {
                    settingsContent = httpClient.GetStringAsync(settingsUrl).GetAwaiter().GetResult();
                }
            }
            else
            {
                using (var reader = File.OpenText(settingsUrl))
                {
                    settingsContent = reader.ReadToEndAsync().GetAwaiter().GetResult();
                }
            }

            var settings = JsonConvert.DeserializeObject<AppSettings>(settingsContent);

            services.AddSingleton(settings.CexIoAdapterService);

            services.AddEmptyLykkeLogging();

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration(
                    _swaggerOptions.ApiVersion,
                    $"{_swaggerOptions.ApiTitle} Under Test");
            });
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
