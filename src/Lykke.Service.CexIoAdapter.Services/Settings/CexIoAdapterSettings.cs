using System.Collections.Generic;
using Lykke.Common.ExchangeAdapter.Server.Settings;

namespace Lykke.Service.CexIoAdapter.Services.Settings
{
    public sealed class CexIoAdapterSettings
    {
        public DbSettings Db { get; set; }

        public OrderBookSettings OrderBooks { get; set; }

        public IReadOnlyCollection<InternalApiCredentials> Clients { get; set; }

        public RabbitMq RabbitMq { get; set; }

        public OrderBookProcessingSettings ToCommonSettings()
        {
            return new OrderBookProcessingSettings
            {
                OrderBooks = new RmqOutput
                {
                    Enabled = RabbitMq.OrderBooks.Enabled,
                    ConnectionString = RabbitMq.OrderBooks.ConnectionString,
                    Durable = false,
                    Exchanger = RabbitMq.OrderBooks.Exchanger
                },
                TickPrices = new RmqOutput
                {
                    Enabled = RabbitMq.TickPrices.Enabled,
                    ConnectionString = RabbitMq.TickPrices.ConnectionString,
                    Durable = false,
                    Exchanger = RabbitMq.TickPrices.Exchanger
                },
                AllowedAnomalisticAssets = new string[0],
                OrderBookDepth = 100,
                MaxEventPerSecondByInstrument = OrderBooks.MaxEventPerSecondByInstrument
            };
        }
    }
}
