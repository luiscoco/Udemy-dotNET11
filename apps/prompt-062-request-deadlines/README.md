# Cancellation and deadlines / API, database and outbound request

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/standard/threading/cancellation-in-managed-threads
- https://learn.microsoft.com/dotnet/standard/data/sqlite/async

The app starts a real local Kestrel API and peer endpoint, executes SELECT 42 through Microsoft.Data.Sqlite 11.0.0, and forwards tokens to both database APIs and HttpClient. SQLite does not provide true asynchronous I/O; its async APIs execute synchronously. The deliberately slow database case is an explicitly labelled cancellable wait before the SQL call, not an interrupted long-running SQLite query. Initial restore needs nuget.org. The API links RequestAborted and its own deadline; HTTP 408 is this exercise's chosen deadline response.
