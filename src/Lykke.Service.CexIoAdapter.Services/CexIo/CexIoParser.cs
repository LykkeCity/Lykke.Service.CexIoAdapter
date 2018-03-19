using System;
using System.Collections.Generic;
using System.Text;
using Common.Log;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lykke.Service.CexIoAdapter.Services.CexIo
{
    public sealed class CexIoParser
    {
        private readonly ILog _log;
        private static readonly IReadOnlyDictionary<string, Func<JToken, IMessageContent>> KnownTypes;

        public CexIoParser(ILog log)
        {
            _log = log;
        }

        private static T Empty<T>(JToken x) where T : IMessageContent, new()
        {
            return new T();
        }

        static CexIoParser()
        {
            KnownTypes =
                new Dictionary<string, Func<JToken, IMessageContent>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {Events.Ping, Empty<Ping>},
                    {Events.Connected, Empty<Connected>},
                    {Events.Auth, NonEmpty<AuthenticationResult>},
                    {Events.OrderBookSubscribe, NonEmpty<OrderBookSubscription>},
                    {Events.OrderBookUpdate, NonEmpty<OrderBookUpdate>},
                    {Events.OrderBookUnsubscribe, NonEmpty<OrderBookUnsubscribed>},
                    {Events.Disconnecting, Empty<Disconnecting>},
                };
        }

        private static T NonEmpty<T>(JToken arg) where T : IMessageContent
        {
            return arg.ToObject<T>();
        }

        public CexIoResponse Parse(byte[] message)
        {
            if (message.Length == 0) return null;

            CexIoResponse envelope;

            try
            {
                envelope = JsonConvert.DeserializeObject<CexIoResponse>(Encoding.UTF8.GetString(message));
            }
            catch (Exception)
            {
                Warning($"Cannot read JSON from bytes: {Convert.ToBase64String(message)}");
                return null;
            }

            if (KnownTypes.TryGetValue(envelope.Event, out var parser))
            {
                try
                {
                    envelope.Message = parser(envelope.Json);
                }
                catch (Exception)
                {
                    Warning($"Cannot parse event of type [{envelope.Event}]: {envelope.Json}");
                }
            }
            else
            {
                Warning($"Received event of unknown type [{envelope.Event}]");
            }

            return envelope;
        }

        private void Warning(string message)
        {
            _log.WriteWarning(nameof(CexIoParser), nameof(CexIoParser), message);
        }
    }
}
