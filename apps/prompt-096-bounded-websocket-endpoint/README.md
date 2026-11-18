# WebSocket / heartbeat messages, aggregate limits and backpressure

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/fundamentals/websockets
- https://learn.microsoft.com/dotnet/api/system.threading.channels.channel

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

The 32-byte limit is a deliberately small aggregate message limit, not a receive-buffer limit. A test gate pauses the sender to demonstrate real channel backpressure; production should not keep this gate closed. The application heartbeat is separate from WebSocket transport keepalive. This fixture does not implement authentication, origin policy or a heartbeat acknowledgement timeout for deployment.
