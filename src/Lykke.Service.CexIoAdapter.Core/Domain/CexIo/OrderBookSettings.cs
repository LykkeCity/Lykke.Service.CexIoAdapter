using System;

namespace Lykke.Service.CexIoAdapter.Core.Domain.CexIo
{
    public sealed class OrderBookSettings
    {
        public bool Enabled { get; set; }
        public ApiCredentials WebSocketCredentials { get; set; }
        public int MaxEventPerSecondByInstrument { get; set; }
        public CurrencyMapping CurrencyMapping { get; set; }
        public TimeoutSettings Timeouts { get; set; }
    }

    public static class OrderBookSettingsExtensions
    {
        public static TimeSpan? Frequency(this OrderBookSettings settings)
        {
            if (settings.MaxEventPerSecondByInstrument == 0)
            {
                return null;
            }

            if (settings.MaxEventPerSecondByInstrument < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(settings.MaxEventPerSecondByInstrument),
                    "MaxEventPerSecondByInstrument should be positive");
            }

            return TimeSpan.FromSeconds(1) / settings.MaxEventPerSecondByInstrument;
        }
    }
}
