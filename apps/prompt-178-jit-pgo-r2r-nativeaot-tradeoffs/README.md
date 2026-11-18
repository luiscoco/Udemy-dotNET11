# Compare JIT, tiering, PGO, ReadyToRun and Native AOT

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/deploying/native-aot/
- https://learn.microsoft.com/dotnet/core/deploying/ready-to-run
- https://learn.microsoft.com/dotnet/core/runtime-config/compilation

Requires Windows x64 and the Native AOT C++ toolchain. Managed and ReadyToRun outputs are self-contained. Five profiles run three times each. The harness sets tiering and PGO flags explicitly and records dynamic-code capabilities, output size, checksum, process wall time and warmed kernel time. These observations demonstrate deployment tradeoffs for this workload. They cannot establish which compiler tier executed every sampled method. Published binaries are rebuilt by the exercise.
