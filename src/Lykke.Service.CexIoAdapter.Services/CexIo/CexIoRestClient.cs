using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Lykke.Service.CexIoAdapter.Core.Domain.SharedContracts;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi;
using Lykke.Service.CexIoAdapter.Services.Tools;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.CexIoAdapter.Services.CexIo
{
    public class CexIoRestClient
    {
        private readonly IApiCredentials _credentials;
        private readonly CurrencyMapping _currencyMapping;

        private static readonly HttpClient Client = new HttpClient
        {
            BaseAddress = new Uri("https://cex.io/api/")
        };

        private const string Limit = "limit";

        public CexIoRestClient(IApiCredentials credentials, CurrencyMapping currencyMapping)
        {
            _credentials = credentials;
            _currencyMapping = currencyMapping;
        }

        public async Task<OrderBookSnapshot> GetOrderBook(
            string instrument,
            CancellationToken ct = default(CancellationToken))
        {
            var (symbol1, symbol2) = CexIoInstrument.FromInstrument(instrument);

            using (var r = await Client.GetAsync($"order_book/{symbol1}/{symbol2}", ct))
            {
                return await r.Content.ReadAsAsync<OrderBookSnapshot>(ct);
            }
        }

        public Task<WalletsResponse> GetBalance(CancellationToken ct = default(CancellationToken))
        {
            return WithNonce(async nonce =>
            {
                var cmd = EmptyRequest(nonce);

                using (var response = await Client.PostAsJsonAsync("balance/", cmd, ct))
                {
                    var obj = await response.Content.ReadAsAsync<JObject>(ct);

                    EnsureNoErrorProperty(obj);

                    return new WalletsResponse
                    {
                        Wallets = ParseAmounts(obj).ToArray()
                    };
                }
            });
        }

        private static void EnsureNoErrorProperty(JObject jObject)
        {
            if (jObject["error"] != null)
            {
                throw new Exception($"Error in response: {jObject["error"]}");
            }
        }

        private static IEnumerable<Wallet> ParseAmounts(JObject response)
        {
            var exclude = new[] {"timestamp", "username"};

            foreach (var kv in response)
            {
                if (exclude.Contains(kv.Key)) continue;

                var balance = kv.Value.ToObject<Balance>();

                yield return new Wallet
                {
                    Asset = kv.Key,
                    Balance = balance.Available,
                    Reserved = balance.Orders ?? 0M
                };
            }
        }

        private EmptyRequest EmptyRequest(long nonce)
        {
            return new EmptyRequest(_credentials, nonce);
        }

        public async Task<IReadOnlyCollection<CurrencyLimitInfo>> GetCurrencyLimits(
            CancellationToken ct = default(CancellationToken))
        {
            var response = await Client.GetAsAsync<CurrencyLimitsResponse>("currency_limits", ct);
            if (!string.IsNullOrEmpty(response.Error)) throw new Exception($"Erroneous response: {response.Error}");
            return response.Data.Pairs;
        }

        private Task<T> WithNonce<T>(Func<long, Task<T>> code)
        {
            return EpochNonce.Lock(_credentials.UserId, code);
        }

        public Task<IReadOnlyCollection<Order>> GetOpenOrdersByInstrument(
            string instrument,
            CancellationToken ct = default(CancellationToken))
        {
            return WithNonce<IReadOnlyCollection<Order>>(async nonce =>
            {
                var (symbol1, symbol2) = CexIoInstrument.FromInstrument(instrument);

                var cmd = new EmptyRequest(_credentials, nonce);

                using (var response = await Client.PostAsJsonAsync($"open_orders/{symbol1}/{symbol2}", cmd, ct))
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsAsync<ShortOrder[]>(ct);
                    return result.Select(x => x.ToOrder(_currencyMapping, Limit)).ToArray();
                }
            });
        }

        public Task<IReadOnlyCollection<Order>> GetOpenOrders(CancellationToken ct = default(CancellationToken))
        {
            return WithNonce<IReadOnlyCollection<Order>>(async nonce =>
            {
                var cmd = new EmptyRequest(_credentials, nonce);

                using (var response = await Client.PostAsJsonAsync("open_orders/", cmd, ct))
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsAsync<ShortOrder[]>(ct);
                    return result.Select(x => x.ToOrder(_currencyMapping, Limit)).ToArray();
                }
            });
        }

        /// <summary>
        /// Returns null if order doesn't exist
        /// </summary>
        public Task<Order> GetOrder(string id, CancellationToken ct = default(CancellationToken))
        {
            return WithNonce(async nonce =>
            {
                var cmd = new GetOrderRequest(id, _credentials, nonce);
                var response = await Client.PostAsJsonAsync("get_order/", cmd, ct);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsAsync<FullOrder>(ct);
                return result?.ToOrder(_currencyMapping, Limit);
            });
        }

        public Task<ShortOrder> PlaceOrder(
            string instrument,
            PlaceOrderCommand request,
            CancellationToken ct = default(CancellationToken))
        {
            return WithNonce(async nonce =>
            {
                var cmd = new PlaceOrderRequest(_credentials, nonce)
                {
                    Amount = request.Amount,
                    OrderType = request.OrderType,
                    Price = request.Price
                };

                var (symbol1, symbol2) = CexIoInstrument.FromInstrument(instrument);

                using (var response = await Client.PostAsJsonAsync($"place_order/{symbol1}/{symbol2}", cmd, ct))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsAsync<ShortOrder>(ct);
                }
            });
        }
    }
}
