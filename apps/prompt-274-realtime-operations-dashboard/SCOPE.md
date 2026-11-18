# Executable capstone scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual authenticated SignalR hub and Microsoft SignalR JavaScript client 10.0.0 run in Chrome against ASP.NET 11. A bounded Channel feeds a BackgroundService, which updates state, invalidates IMemoryCache and publishes to the claim-derived tenant group. An actual MeterListener and ActivityListener observe completion and correlated producer/consumer traces. The controlled defect preserves a stale cache entry. In-memory queue/state are intentionally not durable; monitoring export, reconnect replay, production login and horizontal scaling are outside this bounded integration.
