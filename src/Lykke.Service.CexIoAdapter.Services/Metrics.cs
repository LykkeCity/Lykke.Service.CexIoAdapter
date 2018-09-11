using Nexogen.Libraries.Metrics.Prometheus;

namespace Lykke.Service.CexIoAdapter.Services
{
    public static class Metrics
    {
        public static readonly PrometheusMetrics Prometheus = new PrometheusMetrics();
    }
}
