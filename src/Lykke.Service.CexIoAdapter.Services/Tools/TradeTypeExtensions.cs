using System;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi;

namespace Lykke.Service.CexIoAdapter.Services.Tools
{
    public static class TradeTypeExtensions
    {
        public static OrderType ToOrderType(this TradeType tradeType)
        {
            switch (tradeType)
            {
                case TradeType.Buy: return OrderType.Buy;
                case TradeType.Sell: return OrderType.Sell;
                default: throw new ArgumentOutOfRangeException(nameof(tradeType));
            }
        }
    }
}
