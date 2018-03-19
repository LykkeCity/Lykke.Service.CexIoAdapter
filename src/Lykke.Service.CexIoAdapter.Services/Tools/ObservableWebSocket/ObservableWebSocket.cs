using System;
using System.IO;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;

namespace Lykke.Service.CexIoAdapter.Services.Tools.ObservableWebSocket
{
    public sealed class ObservableWebSocket : IObservable<ISocketEvent>
    {
        private sealed class MessageReceived : IMessageReceived<byte[]>
        {
            public MessageReceived(WebSocketSession session, byte[] content)
            {
                Content = content;
                Session = session;
            }

            public byte[] Content { get; }
            public WebSocketSession Session { get; }
        }

        private readonly string _url;
        private readonly ILog _log;
        private readonly WebSocketTimeouts _timeouts;
        private readonly IObservable<ISocketEvent> _messages;

        public ObservableWebSocket(string url, ILog log, WebSocketTimeouts? timeouts = null)
        {
            _url = url;
            _log = log;
            _timeouts = timeouts ?? WebSocketTimeouts.Default;
            _messages = Observable.Create<ISocketEvent>(async (obs, ct) => { await ReadBytesLoop(obs, log, ct); });
        }

        public IDisposable Subscribe(IObserver<ISocketEvent> observer)
        {
            return _messages.Subscribe(observer);
        }

        private async Task ReadBytesLoop(IObserver<ISocketEvent> obs, ILog log, CancellationToken ct)
        {
            using (var ws = new ClientWebSocket())
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    cts.CancelAfter(_timeouts.ConnectTimeout);

                    await ws.ConnectAsync(new Uri(_url), cts.Token);
                    Info($"Connected to {_url}");
                }

                using (var session = new WebSocketSession(ws, log))
                {
                    obs.OnNext(new SocketConnected(session));

                    var buffer = new byte[4096];

                    var closeSignalReceived = false;

                    while (!ct.IsCancellationRequested && !closeSignalReceived)
                    {
                        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                        {
                            cts.CancelAfter(_timeouts.ReadTimeout);

                            using (var ms = new MemoryStream())
                            {
                                WebSocketReceiveResult result;

                                do
                                {
                                    result = await ws.ReceiveAsync(buffer, cts.Token);

                                    if (result.MessageType == WebSocketMessageType.Close)
                                    {
                                        Info("Close signal received");
                                        obs.OnCompleted();
                                        closeSignalReceived = true;
                                        break;
                                    }

                                    ms.Write(buffer, 0, result.Count);
                                } while (!result.EndOfMessage);

                                obs.OnNext(new MessageReceived(session, ms.ToArray()));
                            }
                        }
                    }
                }
            }
        }

        private void Info(string message)
        {
            _log.WriteInfo(nameof(ObservableWebSocket), "", message);
        }
    }
}
