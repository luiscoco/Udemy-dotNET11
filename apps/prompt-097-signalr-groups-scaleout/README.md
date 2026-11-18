# SignalR / signed identities, room authorization and a real Redis backplane

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/signalr/authn-and-authz
- https://learn.microsoft.com/aspnet/core/signalr/redis-backplane
- https://learn.microsoft.com/aspnet/core/signalr/groups

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

Start a local Redis service before running this lesson, for example docker run --rm -p 127.0.0.1:6397:6379 redis:7.4-alpine. COURSE_REDIS can select another authorized local endpoint. The verification actually runs two Kestrel hosts and cross-node group delivery. Each run uses an isolated Redis channel prefix. Production Redis requires protected networking and credentials; Azure SignalR is a managed alternative. Routing affinity and reconnect/group reauthorization still need deliberate deployment design. The JWT key is ephemeral and generated locally, never a production secret.
