namespace Lykke.Service.CexIoAdapter.Core.Domain.CexIo
{
    public interface IApiCredentials
    {
        string ApiKey { get; }
        string ApiSecret { get; }
        string UserId { get; }
    }
}