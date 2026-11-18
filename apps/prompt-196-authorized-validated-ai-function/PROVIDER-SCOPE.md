# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

The Qwen 0.5B teaching model is not used to claim autonomous tool calling. The sample exports and actually invokes AIFunctionFactory-created functions through the same execution boundary a function-invocation pipeline would use. Calls are deterministic authorization probes, with real side effects counted. A separate live chat explains the application scenario. Production identity must come from trusted authentication, never model arguments.
