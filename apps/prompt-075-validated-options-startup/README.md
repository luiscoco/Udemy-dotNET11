# Typed options / bind and reject invalid startup settings

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options?view=aspnetcore-11.0

The URL is a synthetic configuration value and is not contacted. Data annotations validate required and range rules, while the explicit predicate validates this application's HTTPS contract. ValidateOnStart makes an invalid configuration fail during host startup.
