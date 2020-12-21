using Autofac;
using JetBrains.Annotations;
using Lykke.Service.CexIoAdapter.Services;
using Lykke.Service.CexIoAdapter.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.Hosting;

namespace Lykke.Service.CexIoAdapter.Modules
{
    [UsedImplicitly]
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
        }
    }
}
