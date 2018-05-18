namespace Lykke.Service.CexIoAdapter.Core.Domain.CexIo
{
    public class ApiCredentials : IApiCredentials
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string UserId { get; set; }
    }
}