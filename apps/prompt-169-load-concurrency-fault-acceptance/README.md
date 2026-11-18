# Load, concurrency and fault-injection acceptance tests

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/test/integration-tests
- https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing-concepts

The suite sends real HTTP traffic to a loopback Kestrel service. Two requests deliberately hold both slots while eight excess requests verify overload rejection. It then injects an explicit fault and verifies recovery. Acceptance criteria are functional concurrency and status outcomes. Measured latency belongs to this gated local fixture. Production load capacity requires a representative deployment and workload.
