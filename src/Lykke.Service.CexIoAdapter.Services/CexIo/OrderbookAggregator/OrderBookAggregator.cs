using System;
using System.Linq;
using Common.Log;
using Lykke.Common.ExchangeAdapter.Contracts;
using Lykke.Common.Log;
using Lykke.Service.CexIoAdapter.Services.CexIo.Models.WebSocketApi;

namespace Lykke.Service.CexIoAdapter.Services.CexIo.OrderbookAggregator
{
    public class OrderBookAggregator
    {
        public OrderBookAggregator(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        public OrderBook OrderBook;
        private long _version;
        private string _instrument;
        private string _exchangeName;
        private ILog _log;

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

        public static OrderBookAggregator CreateDefault(ILogFactory lf, string exchangeName, string instrument)
        {
            return new OrderBookAggregator(lf)
            {
                OrderBook = null,
                _version = 0,
                _exchangeName = exchangeName,
                _instrument = instrument
            };
        }

        public OrderBookAggregator ApplyChange(IOrderBookMessage ev)
        {
            if (ev is OrderBookSubscription subscription && OrderBook == null)
            {
                _log.Info(
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
