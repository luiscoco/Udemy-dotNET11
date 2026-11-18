# Typed HTTP clients / factory lifetime and connection pooling

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory
- https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines

The app starts a real loopback HTTP server and alternates requests across two transient typed clients. HTTP/1.1 is forced so MaxConnectionsPerServer is observable as this fixture's request-concurrency limit; HTTP/2 multiplexing differs. The handler is retained by the factory while its connections use finite pooled and idle lifetimes. These demonstration settings require workload-specific tuning.
