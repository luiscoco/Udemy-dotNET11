# Health checks / liveness, readiness and dependency states

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-11.0

The required dependency probe performs a real HTTP call to a separate local route. Startup and optional cache state are controlled fixtures. This application deliberately maps Degraded to HTTP 200; other readiness policies may choose differently. Response JSON exposes check names and statuses, not credentials, response bodies or exception details. These are loopback teaching endpoints, not an authorization design for a public diagnostics service.
