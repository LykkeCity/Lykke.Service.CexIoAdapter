namespace Lykke.Service.CexIoAdapter.Services.Tools.ObservableWebSocket
{
    public sealed class SocketConnected : ISocketEvent
    {
        public SocketConnected(WebSocketSession session)
        {
            Session = session;
        }

        public WebSocketSession Session { get; }
    }
}
