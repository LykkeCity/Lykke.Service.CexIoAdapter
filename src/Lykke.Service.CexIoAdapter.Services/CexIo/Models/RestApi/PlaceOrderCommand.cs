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
}