# Verify hosting support and run a container worker

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide
- https://techcommunity.microsoft.com/blog/appsonazureblog/-net-11-is-now-generally-available-in-azure-functions/4571176
- https://learn.microsoft.com/dotnet/core/extensions/workers

Provider support is recorded from Microsoft documentation checked on 2026-11-13. Azure Functions supports .NET 11 isolated workers with its compatible SDK. This exercise implements the permitted container-worker alternative and actually runs it in local Docker. It creates no Azure resource or deployment. Deduplication is in memory for this bounded synthetic run. Durable production jobs need a persistent idempotency store and checkpoint policy. The image runs as its non-root application user with no network access during verification.
