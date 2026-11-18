# Memory stream adapters / aliases, copies and measured allocations

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/api/system.io.readonlymemorystream?view=net-11.0
- https://learn.microsoft.com/dotnet/api/system.io.writablememorystream?view=net-11.0
- https://learn.microsoft.com/dotnet/api/system.io.stringstream?view=net-11.0

Allocation counters cover synchronous allocations on the current thread, with the payload allocated outside the measured region. Results are fixture measurements, not a whole-process memory or throughput benchmark. Read-only adapters still alias their owners; keep backing memory alive and stable for the adapter lifetime.
