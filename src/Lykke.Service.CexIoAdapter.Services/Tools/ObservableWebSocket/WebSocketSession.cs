using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi;
using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.Tools.ObservableWebSocket
{
    public sealed class WebSocketSession : IDisposable
    {
        private readonly ClientWebSocket _client;
        private readonly ILog _log;
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1);
        private readonly TimeSpan _sendTimeout = TimeSpan.FromSeconds(10);

        public WebSocketSession(ClientWebSocket client, ILog log)
        {
            _client = client;
            _log = log;
        }

        private readonly HashSet<Type> _dontLogCommands = new HashSet<Type> { typeof(PongCommand) };

        public async Task SendAsJson<T>(T cmd)
        {
            try
            {
                await _writeLock.WaitAsync();
                var str = JsonConvert.SerializeObject(cmd);

                if (!_dontLogCommands.Contains(typeof(T)))
                {
                    await _log.WriteInfoAsync(nameof(WebSocketSession), "", $"Sending: {str}");
                }

                using (var cts = new CancellationTokenSource(_sendTimeout))
                {
                    await _client.SendAsync(
                        Encoding.UTF8.GetBytes(str),
                        WebSocketMessageType.Text,
                        true,
                        cts.Token);
                }
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _writeLock?.Dispose();
        }
    }
}
