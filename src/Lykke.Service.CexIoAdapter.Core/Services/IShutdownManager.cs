using System.Threading.Tasks;

namespace Lykke.Service.CexIoAdapter.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
