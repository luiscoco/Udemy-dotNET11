# Extract a module while preserving contracts

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/architecture/microservices/architect-microservice-container-applications/identify-microservice-domain-model-boundaries

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

The module and extracted HTTP service expose the same course lookup contract. Kestrel runs on a real ephemeral loopback port. The fixture stops the service to verify the additional network failure mode. This local extraction exercise uses two hosts in one verification process. Deploying independently requires operational configuration and contract version management.
