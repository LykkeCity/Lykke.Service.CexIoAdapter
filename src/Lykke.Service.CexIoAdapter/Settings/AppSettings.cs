using Lykke.Sdk.Settings;
using Lykke.Service.CexIoAdapter.Services.Settings;

namespace Lykke.Service.CexIoAdapter.Settings
{
    public class AppSettings : BaseAppSettings
    {
        public CexIoAdapterSettings CexIoAdapterService { get; set; }
    }
}
