# ASP.NET Core integration tests with WebApplicationFactory

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/test/integration-tests

WebApplicationFactory exercises the actual ASP.NET Core routing, binding, validation and response pipeline through its in-process TestServer. Each factory receives an independent repository. This lesson does not require a real database. Database-engine verification follows in lesson 165.
