using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Common.Log;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models;
using Lykke.Service.CexIoAdapter.Settings;
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

            return null;
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
                else
                {
                    context.Items[RestClientKey] = new CexIoRestClient(new InternalApiCredentials(), mapping);
                }

                return next();
            });
        }
    }
}
