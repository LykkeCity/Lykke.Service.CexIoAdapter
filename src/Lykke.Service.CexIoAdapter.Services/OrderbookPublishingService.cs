using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.CexIoAdapter.Core.Domain.CexIo;
using Lykke.Service.CexIoAdapter.Core.Domain.SharedContracts;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.RestApi;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi;
using Lykke.Service.CexIoAdapter.Services.CexIo.OrderbookAggregator;
using Lykke.Service.CexIoAdapter.Services.CexIo.WebSocket;
using Lykke.Service.CexIoAdapter.Services.Tools;
using Lykke.Service.CexIoAdapter.Services.Tools.ObservableWebSocket;

namespace Lykke.Service.CexIoAdapter.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class OrderbookPublishingService : IStopable
    {
        private readonly ILog _log;
        private readonly OrderBookSettings _orderBookSettings;
        private readonly RabbitMq _rabbitMq;

        public OrderbookPublishingService(
            ILog log,
            OrderBookSettings orderBookSettings,
            RabbitMq rabbitMq)
        {
            _log = log;
            _orderBookSettings = orderBookSettings;
            _rabbitMq = rabbitMq;
        }

        private IDisposable _subscription;

        public void Start()
        {
            if (_orderBookSettings.Enabled)
            {
                Info($"Starting publishing service, producing events not frequently than one in " +
                     $"{_orderBookSettings.Frequency()} for each instrument");

                var obPublisher = StartRmqPublisher<OrderBook>(_rabbitMq.OrderBooks);
                var tpPublisher = StartRmqPublisher<TickPrice>(_rabbitMq.TickPrices);

                var creds = new ApiCredentials
                {
                    ApiKey = _orderBookSettings.WebSocketCredentials.ApiKey,
                    ApiSecret = _orderBookSettings.WebSocketCredentials.ApiSecret,
                    UserId = _orderBookSettings.WebSocketCredentials.UserId
                };

                var restApi = new CexIoRestClient(creds, _orderBookSettings.CurrencyMapping);

                Info("Retrieving existing pairs");
                var pairs = GetPairs(restApi).Result;
                Info($"{pairs.Count} retrieved");

                var timeouts = new WebSocketTimeouts(
                    readTimeout: _orderBookSettings.Timeouts.SocketInactivity,
                    connectTimeout: _orderBookSettings.Timeouts.WebSocketConnect,
                    writeToSocket: _orderBookSettings.Timeouts.WriteToSocket);

                var wsClient = new CexIoListener(pairs, creds, timeouts, _log);

                var orderbooks = wsClient.Messages
                    .Select(x => x.Message as IOrderBookMessage)
                    .Where(x => x != null)
                    .GroupBy(x => x.Pair)
                    .SelectMany(ProcessSingleInstrument)
                    .Do(_ => { }, err => _log.WriteError("listen-orderbooks", "", err))
                    .RetryWithBackoff(TimeSpan.FromMilliseconds(50), TimeSpan.FromMinutes(1))
                    .Repeat()
                    .Share();

                var ticks = orderbooks
                    .GroupBy(x => x.Asset)
                    .SelectMany(g => g
                        .Select(TickPrice.FromOrderBook)
                        .DistinctUntilChanged())
                    .DistinctUntilChanged();


                var obWorker =
                    _rabbitMq.OrderBooks.Enabled
                        ? orderbooks
                            .GroupBy(x => x.Asset)
                            // throttle each particular asset, not the whole stream
                            .SelectMany(g => g.LimitFrequency(_orderBookSettings.Frequency()))
                            .SelectMany(x => PublishMessageToRabbitMq(x, obPublisher))
                        : Observable.Never<OrderBook>();

                var tbWorker =
                    _rabbitMq.TickPrices.Enabled
                        ? ticks
                            .GroupBy(x => x.Asset)
                            // throttle each particular asset, not the whole stream
                            .SelectMany(g => g.LimitFrequency(_orderBookSettings.Frequency()))
                            .SelectMany(x => PublishMessageToRabbitMq(x, tpPublisher))
                        : Observable.Never<TickPrice>();

                var statPeriod = TimeSpan.FromSeconds(30);

                var obStat = orderbooks
                    .WindowCount(statPeriod)
                    .Sample(statPeriod)
                    .Do(x => Info($"OrderBook updates for last {statPeriod}: {x}"))
                    .Select(_ => new object());

                var ticksStat = ticks
                    .WindowCount(statPeriod)
                    .Sample(statPeriod)
                    .Do(x => Info($"TickPrice updates for last {statPeriod}: {x}"))
                    .Select(_ => new object());

                var workers = new[]
                {
                    obWorker,
                    tbWorker,
                    obStat,
                    ticksStat
                };

                _subscription = new CompositeDisposable(
                    workers.Select(x => x.Subscribe())
                        .Concat(new IDisposable[] {obPublisher, tpPublisher}));
            }
        }

        private async Task<IReadOnlyCollection<(string, string)>> GetPairs(CexIoRestClient restApi)
        {
            Info("Getting currency limits from REST API");
            using (var ct = new CancellationTokenSource(_orderBookSettings.Timeouts.RetrieveCurrencyLimits))
            {
                var limits = await restApi.GetCurrencyLimits(ct.Token);
                var instruments = limits.Select(x => CexIoInstrument.FromPair(x.Symbol1, x.Symbol2));
                Info($"Got instruments: {string.Join(", ", instruments)}");
                return limits.Select(l => (l.Symbol1, l.Symbol2)).ToArray();
            }
        }

        private IObservable<OrderBook> ProcessSingleInstrument(
            IGroupedObservable<string, IOrderBookMessage> currencyStream)
        {
            return CombineWithSnapshot(currencyStream, currencyStream.Key)
                .Sample(TimeSpan.FromSeconds(5));
        }

        private IObservable<OrderBook> CombineWithSnapshot(
            IObservable<IOrderBookMessage> changes,
            string instrument)
        {
            return changes.Scan(
                    OrderBookAggregator.CreateDefault(
                        _log,
                        Name,
                        CexIoInstrument.ToLykkeInstrument(instrument, _orderBookSettings.CurrencyMapping)),
                    (context, ev) => context.ApplyChange(ev))
                .Select(x => x.OrderBook);
        }

        private const string Name = "cex.io";

        private void Info(string message)
        {
            _log.WriteInfo(nameof(OrderbookPublishingService), nameof(OrderbookPublishingService), message);
        }

        private async Task<OrderBookSnapshot> RetrieveOrderbook(string instrument, CexIoRestClient rest)
        {
            Info($"Orderbook for {instrument} is empty, retrieving from rest api");
            var snapshot = await rest.GetOrderBook(instrument);
            Info($"Snapshot for {snapshot.Pair} of version {snapshot.Id} retrieved, " +
                 $"asks: {snapshot.Asks.Length}, " +
                 $"bids: {snapshot.Bids.Length}");

            return snapshot;
        }

        private RabbitMqPublisher<T> StartRmqPublisher<T>(PublishingSettings exchanger)
        {
            var settings = RabbitMqSubscriptionSettings.CreateForPublisher(
                exchanger.ConnectionString,
                exchanger.Exchanger);

            var connection
                = new RabbitMqPublisher<T>(settings)
                    .SetLogger(_log)
                    .SetSerializer(new JsonMessageSerializer<T>())
                    .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                    .PublishSynchronously()
                    .Start();

            return connection;
        }

        private void Error(Exception error)
        {
            _log.WriteError(nameof(OrderbookPublishingService), nameof(OrderbookPublishingService), error);
        }

        private static async Task<T> PublishMessageToRabbitMq<T>(
            T msg,
            IMessageProducer<T> connection)
        {
            await connection.ProduceAsync(msg);
            return msg;
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
