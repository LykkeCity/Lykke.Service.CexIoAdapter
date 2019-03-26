using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Common.Log;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Services.Settings;
using Lykke.Service.CexIoAdapter.Services.Tools;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CexIoAdapter.Controllers
{
    public sealed class SpotController : SpotControllerBase<CexIoRestClient>
    {
        private readonly CurrencyMapping _currencyMapping;
        private readonly ILog _log;

        public SpotController(CexIoAdapterSettings settings, ILogFactory logFactory)
        {
            _currencyMapping = settings.OrderBooks.CurrencyMapping;
            _log = logFactory.CreateLog(this);
        }

        public override Task<GetWalletsResponse> GetWalletBalancesAsync()
        {
            return Api.GetBalance();
        }

        [HttpGet("getInstruments")]
        public async Task<IReadOnlyCollection<string>> GetInstruments()
        {
            var limits = await Api.GetCurrencyLimits();
            return limits
                .Select(x => CexIoInstrument.FromPair(x.Symbol1, x.Symbol2))
                .Select(x => CexIoInstrument.ToLykkeInstrument(x, _currencyMapping))
                .ToArray();
        }

        public override Task<GetLimitOrdersResponse> GetLimitOrdersAsync()
        {
            return Api.GetOpenOrders();
        }

        public override async Task<OrderIdResponse> CreateLimitOrderAsync([FromBody] LimitOrderRequest request)
        {
            var shortOrder = await Api.PlaceOrder(request.Instrument, request.ToCommand());

            return new OrderIdResponse {OrderId = shortOrder?.Id};
        }

        public override async Task<CancelLimitOrderResponse> CancelLimitOrderAsync([FromBody] CancelLimitOrderRequest request)
        {
            var cancelled = await Api.CancelOrder(request.OrderId);

            if (!cancelled)
            {
                _log.Warning($"The order with id = {request.OrderId} has not been cancelled");
            }

            return new CancelLimitOrderResponse {OrderId = request.OrderId };
        }

        public override Task<OrderModel> LimitOrderStatusAsync(string orderId)
        {
            return Api.GetOrder(orderId);
        }
    }
}
