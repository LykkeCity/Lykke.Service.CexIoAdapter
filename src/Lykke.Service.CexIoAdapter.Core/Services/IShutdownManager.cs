using System.Threading.Tasks;
using Common;

namespace Lykke.Service.CexIoAdapter.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
