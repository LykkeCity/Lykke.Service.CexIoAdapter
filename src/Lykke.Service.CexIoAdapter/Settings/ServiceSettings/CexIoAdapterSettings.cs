using System.Collections.Generic;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;

namespace Lykke.Service.CexIoAdapter.Settings.ServiceSettings
{
    public sealed class CexIoAdapterSettings
    {
        public DbSettings Db { get; set; }

        public OrderBookSettings OrderBooks { get; set; }

        public IReadOnlyCollection<ApiCredentials> Clients { get; set; }

        public RabbitMq RabbitMq { get; set; }
    }
}
