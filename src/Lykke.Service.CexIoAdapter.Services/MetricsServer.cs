using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.Log;
using Microsoft.Extensions.Hosting;
using Nexogen.Libraries.Metrics.Prometheus;

namespace Lykke.Service.CexIoAdapter.Services
{
    public sealed class MetricsServer : IHostedService
    {
        private readonly IObservable<Unit> _worker;
        
        private IDisposable _subscription;

        public MetricsServer(ILogFactory lf)
        {
            var log = lf.CreateLog(this);

            _worker = Observable.Using(
                    () => new HttpListener
                    {
                        Prefixes = {"http://*:3001/"},
                        IgnoreWriteExceptions = true
                    },
                    http =>
                    {
                        var sw = Stopwatch.StartNew();
                        log.Info($"Starting metrics service: {string.Join(" | ", http.Prefixes)}");
                        http.Start();
                        log.Info($"Started in {sw.Elapsed}");

                        return Observable.FromAsync(http.GetContextAsync)
                            .Repeat()
                            .SelectMany(async ctx =>
                            {
                                using (ctx.Response)
                                {
                                    try
                                    {
                                        await Respond(ctx, Metrics.Prometheus);
                                        ctx.Response.Close();
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Warning("Error while responding", ex);
                                    }
                                }

                                return Unit.Default;
                            })
                            .ReportErrors("metrics server", log)
                            .Retry();
                    })
                .ReportErrors("metrics server", log);
        }

        private async Task<Unit> Respond(HttpListenerContext ctx, PrometheusMetrics metrics)
        {
            var requestRawUrl = ctx.Request.Url;

            if (requestRawUrl.PathAndQuery == "/metrics")
            {
                ctx.Response.StatusCode = 200;
                await metrics.Expose(ctx.Response.OutputStream);
            }

            if (requestRawUrl.PathAndQuery == "/healthz")
            {
                ctx.Response.StatusCode = 200;

                using (var sw = new StreamWriter(ctx.Response.OutputStream))
                {
                    await sw.WriteLineAsync("Healthy");
                }
            }
            else
            {
                ctx.Response.StatusCode = 404;
            }

            return Unit.Default;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscription = _worker.Subscribe();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription?.Dispose();
            return Task.CompletedTask;
        }
    }
}
