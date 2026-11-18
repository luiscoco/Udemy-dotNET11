# API versioning / preserve v1 while extending v2

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://dotnet.github.io/aspnet-api-versioning/
- https://www.nuget.org/packages/Asp.Versioning.Http/10.2.3

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

Asp.Versioning.Http 10.2.3 is pinned and tested on the selected .NET 11 runtime. Compatibility here is demonstrated for one additive field and a tolerant System.Text.Json reader; strict clients or behavioral changes require separate contract review. This fixture uses Minimal APIs rather than the controller registration shown in the overview.
