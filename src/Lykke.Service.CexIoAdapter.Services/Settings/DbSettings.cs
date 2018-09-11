using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.CexIoAdapter.Services.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
