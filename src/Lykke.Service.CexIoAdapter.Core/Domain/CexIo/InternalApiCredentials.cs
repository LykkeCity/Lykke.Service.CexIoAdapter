using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.CexIoAdapter.Core.Domain.CexIo
{
    public class InternalApiCredentials : IApiCredentials
    {
        public string InternalApiKey { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string UserId { get; set; }
    }
}
