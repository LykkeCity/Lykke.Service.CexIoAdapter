using System;

namespace Lykke.Service.CexIoAdapter.Services.Tools
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime BaseDateTime =
            DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);

        public static long Epoch(this DateTime dt)
        {
            return (long) (dt - BaseDateTime).TotalSeconds;
        }

        public static DateTime FromEpochMilliSeconds(this long milliseconds)
        {
            return BaseDateTime.AddMilliseconds(milliseconds);
        }

        public static DateTime FromEpochSeconds(this long seconds)
        {
            return BaseDateTime.AddSeconds(seconds);
        }
    }
}
