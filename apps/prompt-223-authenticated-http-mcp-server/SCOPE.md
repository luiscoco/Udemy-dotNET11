# Executable lesson scope

.NET SDK 11.0.100 runs official MCP C# SDK 2.4.0 with protocol 2025-11-25 pinned explicitly. SDK and protocol versions are separate. All data, users, JWT signing keys and side effects are owned teaching fixtures. HTTP endpoints bind only to loopback and validate bearer tokens where demonstrated. This is not a deployed OAuth authorization server.

The official ASP.NET Core MCP integration hosts actual Streamable HTTP. JWT authentication validates issuer, audience, lifetime and an ephemeral signing key. Endpoint and primitive filters are separate. Tokens are issued by an explicit local test fixture; this is not a production OAuth authorization server or deployed HTTPS service.
