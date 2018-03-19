namespace Lykke.Service.CexIoAdapter.Core.Domain.CexIo
{
    public sealed class RabbitMq
    {
        public PublishingSettings OrderBooks { get; set; }
        public PublishingSettings TickPrices { get; set; }
    }
}