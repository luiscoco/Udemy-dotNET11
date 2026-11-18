# API policies / rate limits, compression and personal cache boundaries

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/performance/rate-limit
- https://learn.microsoft.com/aspnet/core/performance/response-compression
- https://learn.microsoft.com/aspnet/core/performance/caching/output

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

X-Test-User selects synthetic local identities only and is not secure authentication. The limit is local to this app instance, not a distributed quota. The personal route explicitly disables output caching. Compression is demonstrated for public non-secret HTTP content; production HTTPS compression requires a deliberate security decision. The raw-byte client does not automatically decompress, so the gzip verification inspects the actual wire representation.
