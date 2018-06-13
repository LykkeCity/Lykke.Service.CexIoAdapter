using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Lykke.Service.CexIoAdapter.Tests
{
    public class some_tests
    {
        [Fact]
        public void order_book_serialization()
        {
            var time = DateTime.UtcNow;

            var orderBook = new OrderBook("Exchange", "BTC:USD", time,
                bids: new[] {new OrderBookItem(12000, 0.0005M),},
                asks: new[] {new OrderBookItem(11000, 0.0004M)});

            var serialized = JsonConvert.SerializeObject(orderBook);

            var parsed = JToken.Parse(serialized);
            Assert.NotEmpty(parsed["source"].Value<string>());
            Assert.NotEmpty(parsed["asset"].Value<string>());
            Assert.Equal(time, parsed["timestamp"].Value<DateTime>());
            Assert.NotEmpty(parsed["asks"][0]["price"].Value<string>());
            Assert.NotEmpty(parsed["asks"][0]["volume"].Value<string>());
        }

        [Fact]
        public async Task get_currency_limits()
        {
            var rest = new CexIoRestClient(new InternalApiCredentials(), _mapping);
            var limits = await rest.GetCurrencyLimits();
            Assert.NotEmpty(limits);
        }


        private readonly CurrencyMapping _mapping = new CurrencyMapping
        {
            Rename = new Dictionary<string, string> {{"BTC", "XBT"}},
            FourCharsCurrencies = new[] {"DASH"}
        };

        private readonly InternalApiCredentials _creds = new InternalApiCredentials
        {
            ApiSecret = "secret",
            ApiKey = "key",
            UserId = "userId"
        };

        [Theory]
        [InlineData("BTC:USD", "XBTUSD")]
        [InlineData("EUR:USD", "EURUSD")]
        [InlineData("DASH:USD", "DASHUSD")]
        [InlineData("BTC:DASH", "XBTDASH")]
        public void cex_io_instruments_to_lykke(string cexIo, string lykke)
        {
            Assert.Equal(lykke, CexIoInstrument.ToLykkeInstrument(cexIo, _mapping));
            Assert.Equal(cexIo, CexIoInstrument.FromLykkeInstrument(lykke, _mapping));
        }

        [Fact]
        public void render_place_order_request()
        {
            var request = new PlaceOrderRequest(_creds, 100500)
            {
                OrderType = OrderType.Sell,
                Amount = 0.00005M,
                Price = 10000.23M
            };

            var serialized = JsonConvert.SerializeObject(request);

            var parsed = JToken.Parse(serialized);

            Assert.Equal("sell", parsed["type"].Value<string>());
        }
    }

}
