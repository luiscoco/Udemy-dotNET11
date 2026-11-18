# Executable preview evaluation scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual .NET 11 DnsResolver queries an owned loopback UDP fixture for typed TXT records and TTL. No system DNS configuration changes or external DNS dependency are required. Actual SocketsHttpHandler.ShouldEvictConnection evaluates a synthetic generation change, and Kestrel connection IDs verify real reuse and replacement. A deliberately disabled eviction decision fails the observed contract. Executed on Windows only; cross-OS API support is documented, not inferred from this run.

Windows requires custom DNS server port 53. This lab binds only loopback UDP port 53, with fresh owned hostnames per run; do not run it when that port is already occupied. It changes no system DNS configuration. Eviction is timer-driven; the lab waits for an actual callback and keeps the normal idle timeout longer than the wait.

