# Benchmark equivalent .NET 10 and .NET 11 workloads

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/deploying/
- https://learn.microsoft.com/dotnet/core/runtime-config/compilation

The .NET 11 SDK compiles the same source for net10.0 and net11.0. Both deploy self-contained for win-x64 with the same tiering, PGO and workstation GC configuration. Seven fresh-process samples alternate order. The report records actual runtimes, operating system, architecture, checksums and samples. Local wall times vary with host load. These measurements describe one workload.
