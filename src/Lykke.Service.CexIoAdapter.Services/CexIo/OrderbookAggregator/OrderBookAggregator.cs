using System;
using System.Collections.Concurrent;
using System.Linq;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Service.CexIoAdapter.Core.Domain.SharedContracts;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi;
using Lykke.SettingsReader;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.OrderbookAggregator
{
    public struct OrderBookAggregator
    {
        public OrderBook OrderBook;
        private long _version;
        private ILog _log;
        private string _instrument;
        private string _exchangeName;

        private void ApplyEventToOrderBook(OrderBookUpdate update)
        {
            if (update == null) throw new ArgumentNullException(nameof(update));

            var newOrderbook = OrderBook.Clone(DateTime.UtcNow);

            foreach (var ask in update.Asks)
            {
                newOrderbook.UpdateAsk(ask[0], ask[1]);
            }

            foreach (var bid in update.Bids)
            {
                newOrderbook.UpdateBid(bid[0], bid[1]);
            }

            OrderBook = newOrderbook;
        }

        public static OrderBookAggregator CreateDefault(ILog log, string exchangeName, string instrument)
        {
            return new OrderBookAggregator
            {
                OrderBook = null,
                _version = 0,
                _log = log,
                _exchangeName = exchangeName,
                _instrument = instrument
            };
        }

        public OrderBookAggregator ApplyChange(IOrderBookMessage ev)
        {
            if (ev is OrderBookSubscription subscription && OrderBook == null)
            {
                _log.WriteInfo(
                    nameof(OrderBookAggregator),
                    "",
                    $"Received OrderBook for instrument {_instrument} snapshot with {subscription.Asks.Count} asks " +
                    $"and {subscription.Bids.Count} bids");

                OrderBook = new OrderBook(
                    _exchangeName,
                    _instrument,
                    DateTime.UtcNow,
                    asks: subscription.Asks.Select(x => new OrderBookItem
                    {
                        Price = x[0],
                        Volume = x[1]
                    }),
                    bids: subscription.Bids.Select(x => new OrderBookItem
                    {
                        Price = x[0],
                        Volume = x[1]
                    }));

                _version = subscription.Id;
            }
            else if (ev is OrderBookUpdate update && OrderBook != null)
            {
                var diff = update.Id - _version;

                if (diff != 1) throw new InvalidOperationException($"Missed {diff} events in orderbook");

                ApplyEventToOrderBook(update);
                _version = update.Id;

            }
            else if (ev is OrderBookUpdate && OrderBook == null)
            {
                throw new InvalidOperationException("Orderbook update received before snapshot");
            }

            return this;
        }
    }
}
