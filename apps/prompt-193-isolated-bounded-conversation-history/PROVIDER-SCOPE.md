# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

The character and turn caps bound this small fixture; production context budgeting should count the selected model tokens and reserve output capacity. User identity is supplied by this deterministic test harness; a production endpoint must derive it from authenticated claims. Isolation is asserted on outgoing messages, not on the model answer.
