# EF Core 11 — Full-text migrations and ranked search

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/ef/core/providers/sql-server/full-text-search

Start the local dependencies from this application folder with:

`docker compose -f infrastructure/compose.yaml up -d sqlserver`

Wait for readiness before verification. The SQL Server image builds its full-text component on first use. These services share a fixed local teaching project and bind only to loopback. The supplied passwords are disposable local development credentials. Do not change the compose project name while other lessons are using it.

Stop and remove this teaching stack with `docker compose -f infrastructure/compose.yaml down` after the lessons.



The installed EF SQL Server provider 11.0.0 fails to scaffold its full-text catalog annotation as a C# literal. The base table migration was generated, and the full-text Up/Down operations use explicitly reviewed SQL with suppressTransaction. The deployment context and snapshot omit unstable full-text annotations; the separate SearchContext retains the native fluent full-text mapping. Do not describe the catalog SQL as automatically scaffolded on this version. The ranked API returns Key and Rank; the source joins those keys to Articles.

The installed EF SQL Server provider 11.0.0 fails to scaffold its full-text catalog annotation as a C# literal. The base table migration was generated, and the full-text Up/Down operations use explicitly reviewed SQL with suppressTransaction. The snapshot and target model retain the fluent full-text configuration. Do not describe the catalog SQL as automatically scaffolded on this version. The ranked API returns Key and Rank; the source joins those keys to Articles.

The same installed provider also reports nondeterministic model changes when its native catalog annotation is compared across model builds. The dedicated DeploymentContext avoids that annotation while its migration explicitly creates the full-text objects. No migration warning is suppressed. The runnable verification checks the stable deployment snapshot before applying it.
