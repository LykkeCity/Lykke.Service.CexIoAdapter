﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.ExchangeAdapter.Server;
using Lykke.Common.ExchangeAdapter.Tools.ObservableWebSocket;
using Lykke.Common.Log;
using Lykke.Service.CexIoAdapter.Services.CexIo;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi;
using Lykke.Service.CexIoAdapter.Services.CexIo.OrderbookAggregator;
using Lykke.Service.CexIoAdapter.Services.CexIo.WebSocket;
using Lykke.Service.CexIoAdapter.Services.Settings;
using Microsoft.Extensions.Hosting;
using Nexogen.Libraries.Metrics;

namespace Lykke.Service.CexIoAdapter.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class OrderbookPublishingService : IHostedService
    {
        private readonly ILog _log;
        private readonly OrderBookSettings _orderBookSettings;

        public OrderbookPublishingService(
            ILogFactory lf,
            CexIoAdapterSettings settings)
        {
            _log = lf.CreateLog(this);
            _logFactory = lf;
            _orderBookSettings = settings.OrderBooks;

            var rawOrderBooks = CalculateOrderBooks(GetRawOrderBooks());

            Session = OrderBooksSession.FromRawOrderBooks(
                rawOrderBooks,
                settings.ToCommonSettings(),
                lf);
        }

        private IObservable<OrderBook> CalculateOrderBooks(IObservable<OrderBook> rawOrderBooks)
        {
            var counter = Metrics.Prometheus.Counter()
                .Name("orderbook_received_count_total")
                .Help("Total counts of order books received from web-socket")
                .LabelNames("source", "asset")
                .Register();

            return rawOrderBooks.Do(ob => counter.Labels(ob.Source, ob.Asset).Increment());
        }

        private IObservable<OrderBook> GetRawOrderBooks()
        {
            var creds = new ApiCredentials
            {
                ApiKey = _orderBookSettings.WebSocketCredentials.ApiKey,
                ApiSecret = _orderBookSettings.WebSocketCredentials.ApiSecret,
                UserId = _orderBookSettings.WebSocketCredentials.UserId
            };

            var restApi = new CexIoRestClient(creds, _orderBookSettings.CurrencyMapping);

            _log.Info("Retrieving existing pairs");
            var pairs = GetPairs(restApi).Result;
            _log.Info($"{pairs.Count} retrieved");

            var timeouts = new WebSocketTimeouts(
                readTimeout: _orderBookSettings.Timeouts.SocketInactivity,
                connectTimeout: _orderBookSettings.Timeouts.WebSocketConnect,
                writeToSocket: _orderBookSettings.Timeouts.WriteToSocket);

            var wsClient = new CexIoListener(pairs, creds, timeouts, _log);

            var orderbooks = wsClient.Messages
                .Select(x => x.Message as IOrderBookMessage)
                .Where(x => x != null)
                .GroupBy(x => x.Pair)
                .SelectMany(ProcessSingleInstrument);
            return orderbooks;
        }

        public OrderBooksSession Session { get; }

        private IDisposable _subscription;
        private readonly ILogFactory _logFactory;

        private async Task<IReadOnlyCollection<(string, string)>> GetPairs(CexIoRestClient restApi)
        {
            _log.Info("Getting currency limits from REST API");
            using (var ct = new CancellationTokenSource(_orderBookSettings.Timeouts.RetrieveCurrencyLimits))
            {
                var limits = await restApi.GetCurrencyLimits(ct.Token);
                var instruments = limits.Select(x => CexIoInstrument.FromPair(x.Symbol1, x.Symbol2));
                _log.Info($"Got instruments: {string.Join(", ", instruments)}");
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
                        _logFactory,
                        Name,
                        CexIoInstrument.ToLykkeInstrument(instrument, _orderBookSettings.CurrencyMapping)),
                    (context, ev) => context.ApplyChange(ev))
                .Select(x => x.OrderBook);
        }

        private const string Name = "cex.io";

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _subscription = new CompositeDisposable(Session.Worker.Subscribe(), Session);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _subscription?.Dispose();
            return Task.CompletedTask;
        }
    }
}
