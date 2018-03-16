using System;
using Common.Log;

namespace Lykke.Service.CexIoAdapter.Client
{
    public class CexIoAdapterClient : ICexIoAdapterClient, IDisposable
    {
        private readonly ILog _log;

        public CexIoAdapterClient(string serviceUrl, ILog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            //if (_service == null)
            //    return;
            //_service.Dispose();
            //_service = null;
        }
    }
}
