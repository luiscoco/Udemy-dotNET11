# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

The real request uses GetResponseAsync<OrderDraft> with the provider JSON schema option. A separate deterministic quantity-99 fixture proves business-rule enforcement; it is not claimed as a model response. JSON schema constrains shape, while application validation protects domain rules.
