# Diagnose a slow application with counters and traces

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters
- https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-trace

Windows x64 with .NET 11 and Node.js. Pinned local diagnostic tools restore from NuGet. The harness attaches only to its owned child, collects real System.Runtime counters and sampled thread-time traces, then stops it. Stack samples estimate wall-clock presence. Cache correctness and computation counts provide deterministic acceptance criteria. Sampling and timings depend on the host.
