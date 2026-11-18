# Asynchronous options validation / cancellation and startup errors

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/compatibility/extensions/11/async-options-validation-sync-access
- https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options?view=aspnetcore-11.0

The selected .NET 11 SDK supplies ValidateAsync and asynchronous ValidateOnStart. The rule performs real asynchronous file I/O and has a labelled cancellable wait for the cancellation fixture. Async-only options must not be read through a synchronous creation path before startup. Reload and newly created snapshot paths have separate validation behavior; this lesson verifies the startup-seeded monitor value.
