namespace Lykke.Service.CexIoAdapter.Services.Settings
{
    public class InternalApiCredentials : IApiCredentials
    {
        public string InternalApiKey { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string UserId { get; set; }
    }
}
