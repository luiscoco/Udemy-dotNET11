# Export OpenTelemetry to an operational dashboard

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://aspire.dev/dashboard/standalone/
- https://opentelemetry.io/docs/languages/dotnet/exporters/

Docker Desktop in Linux-container mode and installed Chrome are required. The exercise starts one uniquely named Aspire dashboard with ephemeral loopback ports and synthetic telemetry. The backend uses anonymous access for this isolated local fixture. A pinned image digest makes the backend reproducible. Playwright opens the real dashboard, verifies the received service and retains screenshots and page text. The harness removes only its own container in finally.
