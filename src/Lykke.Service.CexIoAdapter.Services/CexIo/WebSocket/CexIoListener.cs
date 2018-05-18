using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi;
using Lykke.Service.CexIoAdapter.Services.Tools;
using Lykke.Service.CexIoAdapter.Services.Tools.ObservableWebSocket;
using Newtonsoft.Json;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.WebSocket
{
    public sealed class CexIoListener
    {
        private const string WebSocketApiUrl = "wss://ws.cex.io/ws";
        public readonly IObservable<CexIoResponse> Messages;
        private readonly ILog _log;

        public CexIoListener(
            IReadOnlyCollection<(string, string)> pairs,
            IApiCredentials credentials,
            WebSocketTimeouts timeouts,
            ILog log)
        {
            if (pairs == null) throw new ArgumentNullException(nameof(pairs));
            _log = log;

            var parser = new CexIoParser(log);

            Messages = new ObservableWebSocket(WebSocketApiUrl, log, timeouts)
                .Select(x => x.Convert<byte[], CexIoResponse>(parser.Parse))
                .Where(x => x != null)
                .Do(LogServiceMessages)
                .SelectMany(ev => AuthenticateWhenConnected(ev, credentials))
                .SelectMany(ev => RespondToPings(ev))
                .SelectMany(ev => SubscribeWhenAuthenticated(ev, pairs))
                .Select(x => (x as IMessageReceived<CexIoResponse>)?.Content)
                .Where(x => x != null);
        }

        private void LogServiceMessages(ISocketEvent obj)
        {
            if (obj is IMessageReceived<CexIoResponse> msg)
            {
                if (msg.Content.Message is IOrderBookMessage
                    || msg.Content.Message is Ping)
                {
                    return;
                }

                Info($"Message received: {JsonConvert.SerializeObject(obj)}");
            }
        }

        private async Task<ISocketEvent> RespondToPings(ISocketEvent ev)
        {
            if (ev is IMessageReceived<CexIoResponse> msg)
            {
                if (msg.Content.Message is Ping)
                {
                    await ev.Session.SendAsJson(new PongCommand());
                }
            }

            return ev;
        }

        private async Task<ISocketEvent> SubscribeWhenAuthenticated(
            ISocketEvent ev,
            IEnumerable<(string, string)> pairs)
        {
            if (ev is IMessageReceived<CexIoResponse> messageReceived)
            {
                if (messageReceived.Content.Message is AuthenticationResult authenticated)
                {
                    if (!string.IsNullOrEmpty(authenticated.Error))
                    {
                        throw new Exception($"Authentication failed: {authenticated.Error}");
                    }

                    Info("Sending subscribe commands");

                    foreach (var l in pairs)
                    {
                        await ev.Session.SendAsJson(new SubscribeOrderBooksCommand(l.Item1, l.Item2));
                    }
                }
            }

            return ev;
        }

        private async Task<ISocketEvent> AuthenticateWhenConnected(ISocketEvent ev, IApiCredentials creds)
        {
            if (ev is IMessageReceived<CexIoResponse> mr)
            {
                if (mr.Content.Message is Connected)

                await mr.Session.SendAsJson(
                    new AuthenticateCommand(
                        DateTime.UtcNow.Epoch(),
                        creds.ApiKey,
                        creds.ApiSecret));
            }

            return ev;
        }

        private void Info(string message)
        {
            _log.WriteInfo(nameof(CexIoListener), "", message);
        }
    }
}
