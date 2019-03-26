using System;
using Lykke.Common.ExchangeAdapter.SpotController.Records;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi;

namespace Lykke.Service.CexIoAdapter.Services.Tools
{
    public static class LimitOrderExtensions
    {
        public static PlaceOrderCommand ToCommand(this LimitOrderRequest loRequest)
        {
            if (loRequest == null)
                throw new ArgumentNullException(nameof(loRequest));

            return new PlaceOrderCommand(
                loRequest.Instrument, 
                loRequest.Price, 
                loRequest.Volume, 
                loRequest.TradeType.ToOrderType());
        }
    }
}
