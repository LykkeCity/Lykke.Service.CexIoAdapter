﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Common.Log;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Lykke.Service.CexIoAdapter.Core.Services;
using Lykke.Service.CexIoAdapter.Settings.ServiceSettings;
using Lykke.Service.CexIoAdapter.Services;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.CexIoAdapter.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<CexIoAdapterSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<CexIoAdapterSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // TODO: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            //  builder.RegisterType<QuotesPublisher>()
            //      .As<IQuotesPublisher>()
            //      .WithParameter(TypedParameter.From(_settings.CurrentValue.QuotesPublication))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            // TODO: Add your dependencies here

            builder.RegisterType<OrderbookPublishingService>()
                .AsSelf()
                .As<IStopable>()
                .WithParameter(new TypedParameter(typeof(OrderBookSettings), _settings.CurrentValue.OrderBooks))
                .WithParameter(new TypedParameter(typeof(RabbitMq), _settings.CurrentValue.RabbitMq))
                .SingleInstance();

            builder.RegisterInstance(_settings.CurrentValue.OrderBooks.CurrencyMapping)
                .AsSelf();

            builder.Populate(_services);
        }
    }
}
