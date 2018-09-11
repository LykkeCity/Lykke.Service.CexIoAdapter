using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Service.CexIoAdapter.Services;

namespace Lykke.Service.CexIoAdapter.Controllers
{
    public sealed class OrderBookController : OrderBookControllerBase
    {
        protected override OrderBooksSession Session { get; }

        public OrderBookController(OrderbookPublishingService obService)
        {
            Session = obService.Session;
        }
    }
}
