using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.CexIoAdapter.Core.Services;

namespace Lykke.Service.CexIoAdapter.Services
{
    // NOTE: Sometimes, shutdown process should be expressed explicitly. 
    // If this is your case, use this class to manage shutdown.
    // For example, sometimes some state should be saved only after all incoming message processing and 
    // all periodical handler was stopped, and so on.
    
    public class ShutdownManager : IShutdownManager
    {
        private readonly ILog _log;
        private readonly IEnumerable<IStopable> _stopables;

        public ShutdownManager(ILog log, IEnumerable<IStopable> stopables)
        {
            _log = log;
            _stopables = stopables ;
        }

        public async Task StopAsync()
        {
            // TODO: Implement your shutdown logic here. Good idea is to log every step
            foreach (var item in _stopables)
            {
                item.Stop();
            }

            await Task.CompletedTask;
        }
    }
}
