# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

The retry decorator injects deterministic rate-limit exceptions at the IChatClient boundary; the successful third attempt forwards to actual Ollama inference. They are not claimed as live provider rate limits. The policy caps attempts and Retry-After delay within an overall cancellation deadline. Cancellation is never retried. Production transport adapters must classify their documented provider exceptions, review jitter and avoid retrying side-effecting tool execution.
