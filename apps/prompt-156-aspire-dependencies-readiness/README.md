# Aspire resource dependencies and readiness gates

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://aspire.dev/testing/write-your-first-test/
- https://aspire.dev/fundamentals/health-checks/
- https://aspire.dev/fundamentals/service-discovery/
- https://aspire.dev/fundamentals/telemetry/

Docker Desktop with Linux containers is required. The Aspire test harness starts the actual AppHost, temporary PostgreSQL and Redis containers, API and background worker. Each run disposes its owned resources. The fixture uses local synthetic data. OpenTelemetry is verified with a registered in-process processor observing instrumented ASP.NET request activities. Cloud deployment and an external telemetry collector are separate integration steps.
