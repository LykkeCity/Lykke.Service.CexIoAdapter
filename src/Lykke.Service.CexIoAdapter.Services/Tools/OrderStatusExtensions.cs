using System;
using Lykke.Common.ExchangeAdapter.SpotController.Records;

namespace Lykke.Service.CexIoAdapter.Services.Tools
{
    public static class OrderStatusExtensions
    {
        public static OrderStatus ToOrderStatus(this string cexioOrderStatus)
        {
            switch (cexioOrderStatus)
            {
                case "d": return OrderStatus.Fill;
                case "c": return OrderStatus.Canceled;
                case "cd":
                case "a":
                    return OrderStatus.Active;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cexioOrderStatus));
            }
        }
    }
}
