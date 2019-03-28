using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Newtonsoft.Json;
using Xunit;

namespace Lykke.Service.CexIoAdapter.Tests
{
    public class CexioIntegrationTests : IClassFixture<CexioIntegrationWebApplicationFactory>
    {
        private readonly CexioIntegrationWebApplicationFactory _factory;

        public CexioIntegrationTests(CexioIntegrationWebApplicationFactory factory)
        {
            _factory = factory;
        }

        //[Fact]
        public async Task CanGetWalletBalances()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/spot/getWallets");

            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }

        //[Fact]
        public async Task CanGetInstruments()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/spot/getInstruments");

            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }

        //[Fact]
        public async Task CanGetLimitOrders()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/spot/GetLimitOrders");

            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }

        //[Fact]
        public async Task CanPlaceAndCancelLimitOrder()
        {
            var client = _factory.CreateClient();

            var createOrderRequest = new LimitOrderRequest
                {TradeType = TradeType.Buy, Instrument = "BTCUSD", Price = 1500m, Volume = 0.01m};

            // Place new order
            var createResponse = await client.PostAsync(
                "/spot/createLimitOrder",
                new StringContent(
                    JsonConvert.SerializeObject(createOrderRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.True(createResponse.StatusCode == HttpStatusCode.OK);

            var orderIdResponse = await createResponse.Content.ReadAsAsync<OrderIdResponse>();

            var cancelOrderRequest = new CancelLimitOrderRequest {OrderId = orderIdResponse.OrderId};

            // Cancel order
            var cancelResponse = await client.PostAsync(
                "/spot/cancelOrder",
                new StringContent(
                    JsonConvert.SerializeObject(cancelOrderRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.True(cancelResponse.StatusCode == HttpStatusCode.OK);

            var cancelOrderResponse = await cancelResponse.Content.ReadAsAsync<CancelLimitOrderResponse>();

            Assert.Equal(cancelOrderResponse.OrderId, orderIdResponse.OrderId);

            // Cancel same order again, should return 200 OK
            var cancelSubsequentResponse = await client.PostAsync(
                "/spot/cancelOrder",
                new StringContent(
                    JsonConvert.SerializeObject(cancelOrderRequest),
                    Encoding.UTF8,
                    "application/json"));

            Assert.True(cancelSubsequentResponse.StatusCode == HttpStatusCode.OK);

            var cancelOrderSubsequentResponse =
                await cancelSubsequentResponse.Content.ReadAsAsync<CancelLimitOrderResponse>();

            Assert.Equal(cancelOrderSubsequentResponse.OrderId, orderIdResponse.OrderId);

            // Get previously created and cancelled order status
            var statusResponse = await client.GetAsync($"/spot/limitOrderStatus?orderId={orderIdResponse.OrderId}");

            Assert.True(statusResponse.StatusCode == HttpStatusCode.OK);

            var statusOrderResponse = await statusResponse.Content.ReadAsAsync<OrderModel>();

            Assert.Equal(statusOrderResponse.Id, orderIdResponse.OrderId);
            Assert.Equal(OrderStatus.Canceled, statusOrderResponse.ExecutionStatus);
        }
    }
}
