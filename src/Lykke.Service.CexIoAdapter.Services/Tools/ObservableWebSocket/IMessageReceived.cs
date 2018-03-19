namespace Lykke.Service.CexIoAdapter.Services.Tools.ObservableWebSocket
{
    public interface IMessageReceived<out T> : ISocketEvent
    {
        T Content { get; }
    }
}
