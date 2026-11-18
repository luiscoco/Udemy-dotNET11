# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

Capabilities are conservative, tested contracts for these pinned adapter/model combinations, not universal claims about the provider. Both engines execute actual streaming with requested sampling controls; seed does not promise identical cross-engine or cross-version wording. Ollama schema is exercised. This LLamaSharp adapter does not map ChatOptions.ResponseFormat to grammar, so schema is marked unavailable through this common interface; native grammar APIs are out of scope. Tool support is not verified for this small model and is rejected.
