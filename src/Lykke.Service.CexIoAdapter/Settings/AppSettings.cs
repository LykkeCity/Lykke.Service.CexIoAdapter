using Lykke.Service.CexIoAdapter.Settings.ServiceSettings;
using Lykke.Service.CexIoAdapter.Settings.SlackNotifications;

namespace Lykke.Service.CexIoAdapter.Settings
{
    public class AppSettings
    {
        public CexIoAdapterSettings CexIoAdapterService { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
