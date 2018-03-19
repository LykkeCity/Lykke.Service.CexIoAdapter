using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Lykke.Service.CexIoAdapter.Core.Domain.SharedContracts;
using Lykke.Service.CexIoAdapter.Middleware;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.CexIoAdapter.Controllers
{
    [Route("spot")]
    public sealed class SpotController : Controller
    {
        private readonly CurrencyMapping _currencyMapping;
        private CexIoRestClient Rest => this.RestApi();

        public SpotController(CurrencyMapping currencyMapping)
        {
            _currencyMapping = currencyMapping;
        }

        /// <summary>
        /// Returns the current balance for an available asset
        /// </summary>
        [HttpGet("getWallets")]
        [SwaggerOperation("GetWallets")]
        [XApiKeyAuth]
        public Task<WalletsResponse> GetWallets()
        {
            return Rest.GetBalance();
        }

        /// <summary>
        /// Returns all available instruments
        /// </summary>
        [HttpGet("getInstruments")]
        [SwaggerOperation("GetInstruments")]
        public async Task<IReadOnlyCollection<string>> GetInstruments()
        {
            var limits = await Rest.GetCurrencyLimits();
            return limits
                .Select(x => CexIoInstrument.FromPair(x.Symbol1, x.Symbol2))
                .Select(x => CexIoInstrument.ToLykkeInstrument(x, _currencyMapping))
                .ToArray();
        }


        /// <summary>
        /// Returns limit orders
        /// </summary>
        /// <param name="instrument">
        /// List of instrument with “,” as separator. Get orders only by this instruments
        /// </param>
        /// <param name="orderIds">
        /// Get order only with this ids, list of id with “,” as separator
        /// </param>
        [HttpGet("getLimitOrder")]
        [SwaggerOperation("GetLimitOrders")]
        [ProducesResponseType(typeof(IEnumerable<Order>), 200)]
        [XApiKeyAuth]
        public async Task<IActionResult> GetLimitOrders(string instrument, string orderIds)
        {
            if (!string.IsNullOrWhiteSpace(instrument) && !string.IsNullOrWhiteSpace(orderIds))
            {
                return new BadRequestObjectResult("Specify either instrument or orderIds, not both of them");
            }

            if (string.IsNullOrWhiteSpace(instrument) && string.IsNullOrWhiteSpace(orderIds))
            {
                return Ok(await GetAllLimitOrders());
            }

            if (!string.IsNullOrWhiteSpace(instrument))
            {
                return Ok(await GetLimitOrdersByInstrument(instrument.Split(",")));
            }

            if (!string.IsNullOrWhiteSpace(orderIds))
            {
                return Ok(await GetLimitOrdersByOrderId(orderIds.Split(",")));
            }

            // not reachable
            return null;
        }

        private Task<IReadOnlyCollection<Order>> GetAllLimitOrders()
        {
            return Rest.GetOpenOrders();
        }

        private async Task<IReadOnlyCollection<Order>> GetLimitOrdersByOrderId(IReadOnlyCollection<string> orderIds)
        {
            var result = new List<Order>(orderIds.Count);

            foreach (var id in orderIds)
            {
                var order = await Rest.GetOrder(id);

                if (order != null) result.Add(order);
            }

            return result;
        }

        private async Task<IReadOnlyCollection<Order>> GetLimitOrdersByInstrument(IEnumerable<string> instruments)
        {
            var result = new List<Order>();

            foreach (var i in instruments)
            {
                var instrument = CexIoInstrument.FromLykkeInstrument(i, _currencyMapping);
                result.AddRange(await Rest.GetOpenOrdersByInstrument(instrument));
            }

            return result;
        }
    }
}
