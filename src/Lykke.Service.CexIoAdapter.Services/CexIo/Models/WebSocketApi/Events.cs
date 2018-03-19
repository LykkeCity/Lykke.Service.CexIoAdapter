namespace Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi
{
    public static class Events
    {
        public const string Disconnecting = "disconnecting";
        public const string Auth = "auth";
        public const string Connected = "connected";
        public const string OrderBookSubscribe = "order-book-subscribe";
        public const string Pong = "pong";
        public const string Ping = "ping";
        public const string OrderBookUpdate = "md_update";
        public const string OrderBookUnsubscribe = "order-book-unsubscribe";
    }
}
