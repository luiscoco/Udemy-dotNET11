# Database integration tests with isolated PostgreSQL instances

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://dotnet.testcontainers.org/modules/postgres/
- https://www.postgresql.org/docs/current/errcodes-appendix.html

Docker Desktop with Linux containers is required. Each test owns a separate PostgreSQL 17 container with a random host port and disposes it during cleanup. The password is synthetic fixture data. The tests verify PostgreSQL constraints and transaction behavior through Npgsql against the real engine.
