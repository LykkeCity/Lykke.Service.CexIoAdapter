To be done
===

```
[x] Create new project
[x] Register settings and the whole project working
[x] Make simple service working with graceful shutdown
    [x] Service started when application started
    [x] Service shutdown when application shutdown
[x] Deploy application to dev (setup configs, k8s deployments)
[x] Implement publishing cex-io OrderBook to RabbitMQ
    [x] Move logic from previous project
        [ ] WebSocketClient implementation
            [x] Parser
                [x] Authentication
                [x] Connected
                [x] Pings
                [x] OrderBook updates
                [x] OrderBook subscription
                [x] Disconnecting
            [x] Reliability (ping/pong)
            [x] Handling authentication errors
            [x] Handling disconnection
                [x] 0 bytes from socket received (currently it throws NRE)
                [x] Disconnecting event received
            [ ] Exponential timeouts on reconnection
        [x] Orderbook snapshot via RestAPI
        [x] Subscribe to stream
        [x] Aggregation of orderbook changes to orderbooks
        [x] Unsubscribe from stream on disposing subscription
        [x] Matching instruments and cex.io pairs
    [x] OrderBook with copy on change
    [x] Adjust namespaces of all classes and put them to proper projects
    [x] Implement publishing of several instruments (from config)
    [x] Fix for concurrent getting snapshot
    [x] Store OrderBooks to RabbitMQ
    [x] Tickprice
        [x] classes Json-contracts
        [x] Get from OrderBook stream
        [x] Publish to RabbitMQ
[x] Fix error related to stop working on authentication error
[x] Implement authentication and credentials selection according to incoming api-token
    [x] Check if web-client for swagger can send additional headers
[ ] Implement web-api controller as a wrapper over cex-io rest-api client
    [-] Try to reuse existing one
    [x] Fix issue with not incrementing Nonce
    [x] Implement getWallets method
    [x] Implement getInstruments method
    [ ] Implement getLimitOrder
        [x] Mapping from lykke instruments to pairs/cex.io instruments
        [x] Implement getOpenOrders by pair
        [x] Implement getOpenOrders (all)
        [x] Implement getOrderById
        [ ] Check that functionality works
    [ ] Implement createLimitOrder
        [x] Implement restapi-client method
        [ ] Implement Controller's method
    [ ] Implement cancelLimitOrder
    [ ] Implement replaceLimitOrder
[ ] Create service client using autorest
[ ] Publish health issues
    [ ] Last ping/pong from channel
    [ ] Last heartbeat for each instrument
    [ ] Integrate AppInsights
[ ] Simplify WebSocket listener (single loop)
    [x] Main refactoring
    [ ] Reliability
        [x] Reconnects
        [x] Reauthentication
        [x] Resubscribe
        [x] Log non-orderbook messages from cex.io
        [x] Reconnect only when Connected message received from cex.io
        [ ] Handle authentication timeouts
[x] Mapping instruments
    [x] Without `:` in name
    [x] Rename currencies
    [?] Ignore instruments
[ ] Rename throttling setting
[x] Publishing to RabbitMQ
    [x] Allow to specify RabbitMQ for each stream
    [x] Allow to turn-off publishing for each stream
[x] Extract timeouts to config
[x] Extract basic WebSocketListener interface
[x] Ask currency limits once on service start
[ ] Check if it's implemented already or implement by myself - logging all requests/reposnses for System.Net.HttpClient

[ ] CodeReview issues
    [x] A lof of IDisposable instances
    [x] ShutdownManager
    [x] Buffers
        [x] Use websocket API for retrieving snapshots (no need of queues for sync)
        [x] Don't keep all orderbooks in memory just for counting them
    [x] Handle close signal from socket
```

### Legend

```
[x] - done
[ ] - not done
[-] - won't be done
[o] - obsolete (reworked)
[?] - not sure
```