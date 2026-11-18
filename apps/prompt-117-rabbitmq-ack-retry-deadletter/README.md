# RabbitMQ — Acknowledgements, retries and dead letters

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://www.rabbitmq.com/client-libraries/dotnet-api-guide
- https://www.rabbitmq.com/docs/dlx

Start the local dependencies from this application folder with:

`docker compose -f infrastructure/compose.yaml up -d rabbitmq`

Wait for readiness before verification. The SQL Server image builds its full-text component on first use. These services share a fixed local teaching project and bind only to loopback. The supplied passwords are disposable local development credentials. Do not change the compose project name while other lessons are using it.

Stop and remove this teaching stack with `docker compose -f infrastructure/compose.yaml down` after the lessons.

The live processing path uses AsyncEventingBasicConsumer, bounded prefetch, a completion wait and explicit subscription cancellation. BasicGet is used only for bounded redelivery and dead-letter probes. Republishing retries uses publisher confirms before acknowledging the original; a crash in between can still duplicate work. This is not an exactly-once claim.
