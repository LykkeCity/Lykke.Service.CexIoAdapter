using System;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Lykke.Service.CexIoAdapter.Core.Domain.SharedContracts;
using Lykke.Service.CexIoAdapter.Services.Tools;
using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public sealed class ShortOrder
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        [JsonConverter(typeof(EpochConverter))]
        public DateTime Time { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("pending")]
        public decimal Pending { get; set; }

        [JsonProperty("symbol1")]
        public string Symbol1 { get; set; }

        [JsonProperty("symbol2")]
        public string Symbol2 { get; set; }

        public Order ToOrder(CurrencyMapping mapping, string orderType)
        {
            var instrument = CexIoInstrument.FromPair(Symbol1, Symbol2);

            return new Order
            {
                OrderId = Id,
                Instrument = CexIoInstrument.ToLykkeInstrument(instrument, mapping),
                Price = Price,
                Volume = Amount,
                OrderType = orderType,
                TradeType = Type
            };
        }
    }
}
