using System;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Lykke.Service.CexIoAdapter.Core.Domain.SharedContracts;
using Lykke.Service.CexIoAdapter.Services.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public sealed class FullOrder
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("time"), JsonConverter(typeof(EpochConverter))]
        public DateTime Time { get; set; }

        [JsonProperty("lastTxTime"), JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LastTxTime { get; set; }

        [JsonProperty("lastTx")]
        public string LastTx { get; set; }

        [JsonProperty("pos")]
        public string Pos { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("symbol1")]
        public string Symbol1 { get; set; }

        [JsonProperty("symbol2")]
        public string Symbol2 { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("remains")]
        public decimal Remains { get; set; }

        [JsonProperty("tradingFeeMaker")]
        public decimal TradingFeeMaker { get; set; }

        [JsonProperty("tradingFeeTaker")]
        public decimal TradingFeeTaker { get; set; }

        [JsonProperty("tradingFeeStrategy")]
        public string TradingFeeStrategy { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

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
