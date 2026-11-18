# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

The fixture rebuilds a separate 768-dimensional collection from the authoritative live documents, checks completion, atomically updates collection/model/dimension fields together and retains the old 384-dimensional collection for rollback. A production cutover also needs concurrent-write catch-up and a durable document watermark; those are outside this bounded snapshot fixture.
