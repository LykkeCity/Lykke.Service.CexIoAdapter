﻿using Prometheus;

namespace Lykke.Service.CexIoAdapter.Services.Utils
{
    public static class InternalMetrics
    {
        public static readonly Counter OrderBookOutCount = Metrics
            .CreateCounter("order_book_out_count",
                "Counter of sent order books.",
                new CounterConfiguration {LabelNames = new[] {"symbol"}});

        public static readonly Gauge OrderBookOutDelayMilliseconds = Metrics
            .CreateGauge("order_book_out_delay_ms",
                "Gauge of order books delay between receiving and sent in milliseconds.",
                new GaugeConfiguration {LabelNames = new[] {"symbol"}});

        public static readonly Counter QuoteOutCount = Metrics
            .CreateCounter("quote_out_count",
                "Counter of sent quotes.",
                new CounterConfiguration {LabelNames = new[] {"symbol"}});

        public static readonly Gauge QuoteOutSidePrice = Metrics
            .CreateGauge("quote_out_side_price",
                "Gauge of received quote side price.",
                new GaugeConfiguration {LabelNames = new[] {"symbol", "side"}});
    }
}
