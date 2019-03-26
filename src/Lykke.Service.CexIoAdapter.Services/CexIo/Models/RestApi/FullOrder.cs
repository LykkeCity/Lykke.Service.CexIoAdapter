using System;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.CexIoAdapter.Services.Settings;
using Lykke.Service.CexIoAdapter.Services.Tools;
using Newtonsoft.Json;

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

        [JsonProperty("lastTxTime"), JsonConverter(typeof(EpochConverter))]
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

        public OrderModel ToOrder(CurrencyMapping mapping, string orderType)
        {
            var instrument = CexIoInstrument.FromPair(Symbol1, Symbol2);

            return new OrderModel
            {
                Id = Id,
                TradeType = Enum.Parse<TradeType>(Type, true),
                Price = Price,
                Timestamp = Time,
                RemainingAmount = Remains,
                AssetPair = CexIoInstrument.ToLykkeInstrument(instrument, mapping),
                OriginalVolume = Amount,
                ExecutedVolume = Amount - Remains,
                ExecutionStatus = Status.ToOrderStatus()
            };
        }
    }
}
