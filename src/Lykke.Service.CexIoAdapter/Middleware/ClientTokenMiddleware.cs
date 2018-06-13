using System;
using System.Linq;
using Common.Log;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Settings.ServiceSettings;
using Lykke.SettingsReader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CexIoAdapter.Middleware
{
    public static class ClientTokenMiddleware
    {
        private const string RestClientKey = "api-credentials";

        public static CexIoRestClient RestApi(this Controller controller)
        {
            if (controller.HttpContext.Items.TryGetValue(RestClientKey, out var client))
            {
                return (CexIoRestClient) client;
            }

            throw new InvalidOperationException($"Request should be authenticated to use cex-io RestAPI " +
                                                $"(pass {ClientTokenHeader} header with the request)");
        }

        public const string ClientTokenHeader = "X-API-KEY";

        public static void UseAuthenticationMiddleware(
            this IApplicationBuilder app,
            IReloadingManager<CexIoAdapterSettings> settings,
            ILog log)
        {
            var allClients = settings.CurrentValue.Clients.ToDictionary(x => x.InternalApiKey);

            app.Use((context, next) =>
            {
                var mapping = settings.CurrentValue.OrderBooks.CurrencyMapping;

                if (context.Request.Headers.TryGetValue(ClientTokenHeader, out var token))
                {
                    if (allClients.TryGetValue(token[0], out var creds))
                    {
                        if (creds != null)
                        {
                            context.Items[RestClientKey] = new CexIoRestClient(creds, mapping);
                        }
                    }
                }

                return next();
            });
        }
    }
}
