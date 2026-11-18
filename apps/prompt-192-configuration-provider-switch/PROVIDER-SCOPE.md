# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

The second provider executes LLamaSharp 0.30.0 in process on CPU. Set COURSE_AI_GGUF to a compatible GGUF file. The recording uses the same Qwen weights as Ollama, SHA256 c5396e06af294bd101b30dce59131a76d2b773e76950acc870eda801d3ab0515. The model is shared; the adapters and execution engines differ. Common interface does not guarantee feature parity.
