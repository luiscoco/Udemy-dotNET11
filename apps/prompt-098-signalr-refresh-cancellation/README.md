# SignalR in .NET 11 / refresh identity claims and cancel ordinary invocations

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/signalr/authn-and-authz
- https://learn.microsoft.com/aspnet/core/signalr/dotnet-client
- https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-11

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

The example uses the .NET 11 client and server. Automatic refresh is disabled only to make this explicit refresh demonstration deterministic; production may configure automatic scheduling from token lifetime. Ordinary invocation cancellation here uses the .NET client documented capability, not a JavaScript invoke cancellation API. Client cancellation completes before server cleanup, so the test waits for the server probe. Already-running invocations keep their original principal. All JWTs and signing keys are synthetic local fixtures.
