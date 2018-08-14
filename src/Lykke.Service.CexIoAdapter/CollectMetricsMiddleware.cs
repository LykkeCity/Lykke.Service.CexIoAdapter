using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Routing;
using Nexogen.Libraries.Metrics;

namespace Lykke.Service.CexIoAdapter
{

    internal class HttpMetrics
    {
        public ILabelledCounter HttpRequestsTotal { get; protected set; }

        public ILabelledHistogram HttpRequestDurationSeconds { get; protected set; }

        public HttpMetrics(IMetrics m)
        {
            this.HttpRequestsTotal = m.Counter().Name("http_requests_total").Help("The total count of http requests").LabelNames("method", "handler", "code").Register();
            this.HttpRequestDurationSeconds = m.Histogram().Name("http_request_duration_seconds").Buckets(0.0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1.0, 2.5, 5.0, 7.5, 10.0, 30.0, 60.0, 120.0, 180.0, 240.0, 300.0).Help("Total duration of http request").LabelNames("method", "handler", "code").Register();
        }
    }

    internal class CollectMetricsMiddleware
    {
        private readonly RequestDelegate next;
        private readonly HttpMetrics m;

        public CollectMetricsMiddleware(RequestDelegate next, HttpMetrics m)
        {
            this.next = next;
            this.m = m;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            Stopwatch sw = Stopwatch.StartNew();
            await this.next(httpContext);
            sw.Stop();
            string method = httpContext.Request.Method;
            string httpMetricPath = this.GetHttpMetricPath(httpContext);
            string str = httpContext.Response.StatusCode.ToString();
            this.m.HttpRequestDurationSeconds.Labels(method, httpMetricPath, str).Observe(sw.Elapsed.TotalSeconds);
            this.m.HttpRequestsTotal.Labels(method, httpMetricPath, str).Increment();
        }

        /// <summary>
        /// Gets the path from a http context. If the path was handled by routing
        /// middleware we attempt to get the route template falling back to Request.Path
        /// if we are unable to find the template
        /// 
        /// Based on the implementation of <see cref="!:RouterMiddleware" /> define here <a href="https://github.com/aspnet/Routing/blob/dev/src/Microsoft.AspNetCore.Routing/RouterMiddleware.cs" />
        /// when the context matches a route then we add an IRoutingFeature to the <see cref="P:Microsoft.AspNetCore.Http.HttpContext.Features" /> collection with the <see cref="P:Microsoft.AspNetCore.Routing.IRoutingFeature.RouteData" />
        /// set to those provided by the matched route. So to get the last matched route we need to get walk backwards from the <see cref="P:Microsoft.AspNetCore.Routing.RouteData.Routers" /> collection. Because we
        /// are template based we need to check if the router is a <see cref="T:Microsoft.AspNetCore.Routing.RouteBase" /> and if so extract the <see cref="P:Microsoft.AspNetCore.Routing.RouteBase.ParsedTemplate" />.
        /// 
        /// Why not use the last router? because MVC for examples adds it's internal router into the RouteData collection.
        /// </summary>
        /// <param name="context">The current  http context</param>
        /// <returns></returns>
        protected virtual string GetHttpMetricPath(HttpContext context)
        {
            IRoutingFeature routingFeature = context.Features.Get<IRoutingFeature>();
            string str = (string) null;
            if (routingFeature != null)
            {
                RouteBase last = CollectMetricsMiddleware.FindLast(routingFeature.RouteData.Routers);
                if (last != null)
                    str = this.GetRoutePath(context, last);
            }
            return str ?? context.Request.Path.Value.ToLowerInvariant();
        }

        protected virtual string GetRoutePath(HttpContext context, RouteBase lastRouter)
        {
            string str = lastRouter.ParsedTemplate.TemplateText;
            Uri uri = new Uri(context.Request.GetDisplayUrl(), UriKind.Absolute);
            int num = uri.Segments.Length - lastRouter.ParsedTemplate.Segments.Count;
            if (num == 1)
                str = uri.Segments[0] + str;
            else if (num == 2)
                str = uri.Segments[0] + uri.Segments[1] + str;
            else if (num == 3)
                str = uri.Segments[0] + uri.Segments[1] + uri.Segments[2] + str;
            else if (num > 3)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int index = 0; index < num; ++index)
                    stringBuilder.Append(uri.Segments[index]);
                stringBuilder.Append(str);
                str = stringBuilder.ToString();
            }
            return str;
        }

        /// <summary>
        /// Gets the last <see cref="T:Microsoft.AspNetCore.Routing.RouteBase" /> that
        /// matched the request. We use the route base because the <see cref="T:Microsoft.AspNetCore.Routing.IRouter" />
        /// doesn't expose the template text.
        /// </summary>
        /// <param name="routers">List of routers that matched the request</param>
        /// <returns></returns>
        private static RouteBase FindLast(IList<IRouter> routers)
        {
            if (routers == null || routers.Count == 0)
                return (RouteBase) null;
            for (int index = routers.Count - 1; index >= 0; --index)
            {
                if (routers[index] is RouteBase)
                    return routers[index] as RouteBase;
            }
            return (RouteBase) null;
        }
    }
}
