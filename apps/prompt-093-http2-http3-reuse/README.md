# HTTP/2 and HTTP/3 / exact versions and observed connection reuse

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/http3
- https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines

The local TLS certificate is ephemeral and accepted only when its fingerprint matches this fixture. It is never installed as a machine trust root. HTTP/3 requires host QUIC support and loopback UDP. If unavailable, the source still attempts an exact HTTP/3 request and reports the failure; no fallback is presented as an HTTP/3 measurement. The concurrency and delay are controlled teaching load, not a production benchmark.
