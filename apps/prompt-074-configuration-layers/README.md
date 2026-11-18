# Configuration / provider precedence and explicit reload

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/extensions/configuration-providers
- https://learn.microsoft.com/dotnet/core/extensions/options

The prefixed environment variable is set only inside the application process and restored in finally. JSON file watching is enabled; the fixture calls Reload explicitly so its verification does not depend on OS file-watcher timing. A change-token subscription verifies refresh notification. A previously bound POCO is intentionally separate from live configuration.
