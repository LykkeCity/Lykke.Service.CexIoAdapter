using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Services.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.CexIoAdapter.Controllers
{
    public sealed class SpotController : SpotControllerBase<CexIoRestClient>
    {
        private readonly CurrencyMapping _currencyMapping;

        public SpotController(CexIoAdapterSettings settings)
        {
            _currencyMapping = settings.OrderBooks.CurrencyMapping;
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

    }
}
