namespace Lykke.Service.CexIoAdapter.Services.Settings
{
    public sealed class OrderBookSettings
    {
        public bool Enabled { get; set; }
        public ApiCredentials WebSocketCredentials { get; set; }
        public int MaxEventPerSecondByInstrument { get; set; }
        public CurrencyMapping CurrencyMapping { get; set; }
        public TimeoutSettings Timeouts { get; set; }
    }
}
