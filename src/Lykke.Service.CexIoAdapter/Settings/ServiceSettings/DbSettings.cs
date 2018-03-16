using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.CexIoAdapter.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
