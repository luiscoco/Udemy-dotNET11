# Reverse proxy / trusted forwarded headers and request localization

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer
- https://learn.microsoft.com/aspnet/core/fundamentals/localization/select-language-culture

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

The proxy really binds its upstream socket to 127.0.0.2, which alone is trusted by the backend. The direct client uses 127.0.0.1 and is not trusted. One forwarded hop is accepted. The ephemeral TLS certificate is pinned only for this local fixture and never installed as a trust root. Production proxy addresses, network boundaries and host filtering must match the actual deployment; do not trust arbitrary client-provided forwarding headers.
