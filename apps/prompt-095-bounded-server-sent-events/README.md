# Server-sent events / bounded buffering and disconnect cleanup

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses
- https://learn.microsoft.com/aspnet/core/fundamentals/websockets

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

The queue is per connection, capacity two, with a waiting producer. A controlled gate proves backpressure before consuming. A separate held stream makes disconnect cleanup observable. Bounds apply to the application queue, not to total server, framework or network memory. No replay or durable event history is provided.
