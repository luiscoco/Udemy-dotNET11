# Blazor component tests with bUnit

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://bunit.dev/docs/getting-started/writing-tests.html
- https://bunit.dev/docs/interaction/trigger-event-handlers.html

bUnit 2.15.2 renders the actual Razor component and dispatches its event handlers in a component test environment. The external persistence interface is replaced by a recording fake. A real-browser journey is demonstrated separately in lesson 167.
