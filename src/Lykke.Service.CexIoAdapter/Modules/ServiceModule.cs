using Autofac;
using Lykke.Service.CexIoAdapter.Services;
using Lykke.Service.CexIoAdapter.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.Hosting;
using Nexogen.Libraries.Metrics.Prometheus;

namespace Lykke.Service.CexIoAdapter.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<OrderbookPublishingService>()
                .AsSelf()
                .As<IHostedService>()
                .SingleInstance();

            builder.RegisterInstance(_settings.CurrentValue.CexIoAdapterService)
                .AsSelf();

            builder.RegisterInstance(Metrics.Prometheus).As<IExposable>();
        }
    }
}
