﻿using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi
{
    public struct PlaceOrderCommand
    {
        public readonly OrderType OrderType;
        public readonly decimal Amount;
        public readonly decimal Price;
        public readonly string Instrument;

        public PlaceOrderCommand(string instrument, decimal price, decimal amount, OrderType orderType)
        {
            Instrument = instrument;
            Price = price;
            Amount = amount;
            OrderType = orderType;
        }
    }

    public sealed class PlaceOrderRequest : EmptyRequest
    {
        public PlaceOrderRequest(ApiCredentials creds, long nonce) : base(creds, nonce)
        {
        }

        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public OrderType OrderType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }
    }
}
