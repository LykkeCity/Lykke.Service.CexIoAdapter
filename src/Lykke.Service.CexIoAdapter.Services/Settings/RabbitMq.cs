namespace Lykke.Service.CexIoAdapter.Services.Settings
{
    public sealed class RabbitMq
    {
        public PublishingSettings OrderBooks { get; set; }
        public PublishingSettings TickPrices { get; set; }
    }
}