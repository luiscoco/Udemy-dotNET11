# Four render modes in one application

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/blazor/

Install Google Chrome and Node.js. The verifier uses an isolated Chrome session against this loopback-only application. Run npm install --prefix tools when the shared course browser dependency is unavailable. Run dotnet run --project App -- serve to interact with the application.

The Client project contains components that can execute in WebAssembly. Static SSR has no interactive event dispatch. Auto chooses a renderer per visit using runtime availability. The browser evidence records the observed initial renderer and verifies a warmed WebAssembly visit.
