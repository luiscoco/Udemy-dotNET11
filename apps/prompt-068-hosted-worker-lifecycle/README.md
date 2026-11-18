# BackgroundService / startup, errors and graceful shutdown

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-11.0
- https://learn.microsoft.com/dotnet/core/extensions/workers

The real Generic Host owns this BackgroundService. A gate separates startup readiness from completing work. A known job failure is recoverable only under the selected exercise policy; the fatal mode faults ExecuteAsync with StopHost configured. This finite three-job fixture ends its execution loop before the normal host stop, and the stop call still observes the lifecycle and cleanup.
