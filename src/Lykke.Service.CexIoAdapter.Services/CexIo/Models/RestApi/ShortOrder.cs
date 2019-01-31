using System;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.CexIoAdapter.Services.Settings;
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

        public OrderModel ToOrder(CurrencyMapping mapping)
        {
            var instrument = CexIoInstrument.FromPair(Symbol1, Symbol2);

            TradeType type;

            switch (Type?.ToLowerInvariant())
            {
                case "buy":
                    type = TradeType.Buy;
                    break;
                case "sell":
                    type = TradeType.Sell;
                    break;
                default:
                    throw new InvalidOperationException($"Type {Type} is unknown");

            }

            return new OrderModel
            {
                Id = Id,
                Symbol = CexIoInstrument.ToLykkeInstrument(instrument, mapping),
                Price = Price,
                OriginalVolume = Amount,
                TradeType = type,
                Timestamp = Time,
                RemainingAmount = Pending,
                ExecutedVolume = Amount - Pending
            };
        }
    }
}
