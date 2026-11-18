# MTP timeouts, failure limits and per-module reports

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/tools/dotnet-test-mtp
- https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-extensions-test-reports
- https://learn.microsoft.com/dotnet/core/whats-new/dotnet-11/sdk

The two real MSTest modules run through the .NET 11 Microsoft.Testing.Platform mode. Deliberate slow and failing probes are selected only for their policy demonstrations. Verification separately checks actual SDK exit codes 3 and 13. Per-module TRX directories preserve reports with the same requested filename. Framework or process scheduling can allow already-running work to finish after the failure limit is reached.
