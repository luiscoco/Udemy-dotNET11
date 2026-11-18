# Migrations — Inspect deployment SQL and rehearse recovery

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying

The supplied migrations were generated with dotnet-ef 11.0.0 and then inspected. The lab uses only disposable local SQLite files. Production deployment is a separate reviewed operation; SQLite does not support general idempotent migration scripts.

To extend the model with another generated migration, install a local tool manifest from this application folder:

```powershell
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 11.0.0
# Add your compatible model change first.
dotnet ef migrations add YourChange --project App --output-dir Migrations
dotnet ef migrations script --project App --output artifacts/your-reviewed-script.sql
```

Do not apply an unreviewed script to a production database. The supplied exercise applies migrations only to its own disposable fixture. These commands generate new source and a script; they are separate from the automatic verification sequence.
