using System;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi;

namespace Lykke.Service.CexIoAdapter.Services.Tools
{
    public static class OrderTypeExtensions
    {
        public static TradeType ToTradeType(this OrderType orderType)
        {
            switch (orderType)
            {
                case OrderType.Buy: return TradeType.Buy;
                case OrderType.Sell: return TradeType.Sell;
                default: throw new ArgumentOutOfRangeException(nameof(orderType));
            }
        }
    }
}
