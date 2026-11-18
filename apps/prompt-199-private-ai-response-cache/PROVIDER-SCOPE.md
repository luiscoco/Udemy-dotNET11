# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

In-memory single-process teaching cache with a short expiry. User identities and sensitivity classification are trusted fixture inputs; production must derive them from authenticated context and data policy. Hashing a key does not anonymize sensitive content. The sample does not log private prompts or model response content or persist response cache contents.
