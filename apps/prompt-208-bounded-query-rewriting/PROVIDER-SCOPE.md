# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

One actual structured chat request proposes rewrites. The original is preserved, generated strings are validated, and actual database searches are capped at three. The regression supplies twenty explicitly controlled candidate formulations to test the application budget; those extra formulations are not claimed as model output. Rankings are observed, not claimed to improve for every question.
