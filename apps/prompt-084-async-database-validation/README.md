# Async request validation / database lookup and a unique constraint

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/fundamentals/validation
- https://learn.microsoft.com/ef/core/modeling/indexes
- https://learn.microsoft.com/ef/core/providers/sqlite/limitations

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

The controlled gate makes both race lookups complete before either insert. Each validator and handler owns its own DbContext. SQLite uses asynchronous EF APIs but its provider performs underlying I/O synchronously; this fixture does not demonstrate nonblocking database I/O performance. The unique index supplies final consistency. Emails are lowercase synthetic fixtures; production normalization and collision rules must be designed explicitly.
