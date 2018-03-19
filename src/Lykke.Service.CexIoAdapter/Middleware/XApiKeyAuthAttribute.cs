using System.Collections.Generic;
using System.Linq;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lykke.Service.CexIoAdapter.Middleware
{
    public sealed class XApiKeyAuthAttribute : ActionFilterAttribute
    {
        internal static IReadOnlyDictionary<string, ApiCredentials> Credentials { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(ClientTokenMiddleware.ClientTokenHeader, out var h)
            || !h.Any())
            {
                context.Result = new BadRequestObjectResult(
                    $"Header {ClientTokenMiddleware.ClientTokenHeader} is required");
            }

            if (!Credentials.TryGetValue(h.First(), out var creds))
            {
                context.Result = new UnauthorizedResult();
            }

            base.OnActionExecuting(context);
        }
    }
}
