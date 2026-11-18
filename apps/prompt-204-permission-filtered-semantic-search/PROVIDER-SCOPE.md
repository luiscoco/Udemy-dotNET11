# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

Tenant and user values are trusted harness identities. Real endpoints must derive them from authenticated server claims. Parameterized SQL applies both filters before returning candidates. This is an application-query authorization demonstration, not a PostgreSQL row-level-security deployment. Private fixture contents are not logged.
