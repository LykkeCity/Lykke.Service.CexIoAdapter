namespace Lykke.Service.CexIoAdapter.Services.Settings
{
    public interface IApiCredentials
    {
        string ApiKey { get; }
        string ApiSecret { get; }
        string UserId { get; }
    }
}