using System;

namespace Lykke.Service.CexIoAdapter.Core.Domain.CexIo
{
    public sealed class TimeoutSettings
    {
        public TimeSpan SocketInactivity { get; set; }
        public TimeSpan WriteToSocket { get; set; }
        public TimeSpan RetrieveCurrencyLimits { get; set; }
        public TimeSpan WebSocketConnect { get; set; }
    }
}
