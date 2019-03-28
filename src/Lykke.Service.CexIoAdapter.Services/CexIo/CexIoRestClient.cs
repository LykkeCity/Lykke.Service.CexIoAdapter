using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Common.Log;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi;
using Lykke.Service.CexIoAdapter.Services.Settings;
using Lykke.Service.CexIoAdapter.Services.Tools;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.CexIoAdapter.Services.CexIo
{
    public sealed class CexIoRestClient
    {
        private readonly IApiCredentials _credentials;
        private readonly CurrencyMapping _currencyMapping;
        private readonly HttpClient _client;

        private const string Limit = "limit";

        public CexIoRestClient(IApiCredentials credentials, CurrencyMapping currencyMapping, ILogFactory logFactory)
        {
            _credentials = credentials;
            _currencyMapping = currencyMapping;
            _client = new HttpClient(new LoggingHandler(logFactory.CreateLog(this), new HttpClientHandler()))
            {
                BaseAddress = new Uri("https://cex.io/api/")
            };
        }

        public async Task<OrderBookSnapshot> GetOrderBook(
            string instrument,
            CancellationToken ct = default(CancellationToken))
        {
            var (symbol1, symbol2) = CexIoInstrument.FromInstrument(instrument);

            using (var r = await _client.GetAsync($"order_book/{symbol1}/{symbol2}", ct))
            {
                return await r.Content.ReadAsAsync<OrderBookSnapshot>(ct);
            }
        }

        public Task<GetWalletsResponse> GetBalance(CancellationToken ct = default(CancellationToken))
        {
            return EpochNonce.Lock(_credentials.ApiKey, async nonce =>
            {
                var cmd = EmptyRequest(nonce);

                using (var response = await _client.PostAsJsonAsync("balance/", cmd, ct))
                {
                    response.EnsureSuccessStatusCode();

                    var token = await response.Content.ReadAsAsync<JToken>(ct);

                    EnsureSuccessContent(token);

                    return new GetWalletsResponse
                    {
                        Wallets = ParseAmounts(token.ToObject<JObject>()).ToArray()
                    };
                }
            });
        }

        public async Task<IReadOnlyCollection<CurrencyLimitInfo>> GetCurrencyLimits(
            CancellationToken ct = default(CancellationToken))
        {
            var response = await _client.GetAsAsync<CurrencyLimitsResponse>("currency_limits", ct);
            if (!string.IsNullOrEmpty(response.Error)) throw new Exception($"Erroneous response: {response.Error}");
            return response.Data.Pairs;
        }

        public async Task<GetLimitOrdersResponse> GetOpenOrders(CancellationToken ct = default(CancellationToken))
        {
            return await EpochNonce.Lock(_credentials.ApiKey, async nonce =>
            {
                var cmd = new EmptyRequest(_credentials, nonce);

                using (var response = await _client.PostAsJsonAsync("open_orders/", cmd, ct))
                {
                    response.EnsureSuccessStatusCode();

                    var token = await response.Content.ReadAsAsync<JToken>(ct);

                    EnsureSuccessContent(token);

                    var result = token.ToObject<ShortOrder[]>();

                    return new GetLimitOrdersResponse
                    {
                        Orders = result.Select(x => x.ToOrder(_currencyMapping)).ToArray()
                    };
                }
            });
        }

        /// <summary>
        /// Returns null if order doesn't exist
        /// </summary>
        public Task<OrderModel> GetOrder(string id, CancellationToken ct = default(CancellationToken))
        {
            return EpochNonce.Lock(_credentials.ApiKey, async nonce =>
            {
                var cmd = new GetOrderRequest(id, _credentials, nonce);

                var response = await _client.PostAsJsonAsync("get_order/", cmd, ct);

                response.EnsureSuccessStatusCode();

                var token = await response.Content.ReadAsAsync<JToken>(ct);

                EnsureSuccessContent(token);

                var result = token.ToObject<FullOrder>();

                return result?.ToOrder(_currencyMapping, Limit);
            });
        }

        public Task<ShortOrder> PlaceOrder(
            string instrument,
            PlaceOrderCommand request,
            CancellationToken ct = default(CancellationToken))
        {
            return EpochNonce.Lock(_credentials.ApiKey, async nonce =>
            {
                var cmd = new PlaceOrderRequest(_credentials, nonce)
                {
                    Amount = request.Amount,
                    OrderType = request.OrderType,
                    Price = request.Price
                };

                var (symbol1, symbol2) =
                    CexIoInstrument.FromInstrument(CexIoInstrument.FromLykkeInstrument(instrument, _currencyMapping));

                using (var response = await _client.PostAsJsonAsync($"place_order/{symbol1}/{symbol2}", cmd, ct))
                {
                    response.EnsureSuccessStatusCode();

                    var token = await response.Content.ReadAsAsync<JToken>(ct);

                    EnsureSuccessContent(token);

                    return token.ToObject<ShortOrder>();
                }
            });
        }

        public Task<bool> CancelOrder(string orderId, CancellationToken ct = default(CancellationToken))
        {
            return EpochNonce.Lock(_credentials.ApiKey, async nonce =>
            {
                var cmd = new CancelOrderRequest(orderId, _credentials, nonce);

                using (var response = await _client.PostAsJsonAsync("cancel_order/", cmd, ct))
                {
                    response.EnsureSuccessStatusCode();

                    var token = await response.Content.ReadAsAsync<JToken>(ct);

                    var errorToken = token.SelectToken("error");

                    if (errorToken != null)
                    {
                        if (errorToken.ToString().Contains("Order not found"))
                        {
                            // cex.io returns "Order not found" in both cases:
                            // 1 - Order has already been cancelled (this case is being caught here)
                            // 2 - Order doesn't even exist
                            return true;
                        }

                        throw new Exception($"Error in response: {errorToken}");
                    }

                    return (bool) token;
                }
            });
        }

        private static void EnsureSuccessContent(JToken jToken)
        {
            var error = jToken.SelectToken("error");

            if (error != null)
            {
                throw new Exception($"Error in response: {error}");
            }
        }

        private static IEnumerable<WalletBalanceModel> ParseAmounts(JObject response)
        {
            var exclude = new[] { "timestamp", "username" };

            foreach (var kv in response)
            {
                if (exclude.Contains(kv.Key)) continue;

                var balance = kv.Value.ToObject<Balance>();

                yield return new WalletBalanceModel
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
    }
}
