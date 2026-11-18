# Agent provider scope

This app uses real Microsoft Agent Framework 1.28.0 agents and actual Qwen3 0.6B local CPU model inference via OllamaSharp 5.5.2 and Microsoft.Extensions.AI 11.0.0. Model text is untrusted and may vary. Exact package compatibility is verified by build and execution. Tool calling is actually exercised where claimed; deterministic adversarial inputs and human decisions are explicitly labelled fixtures.

Two actual dotnet child processes write and restore the framework session. The assertion inspects the actual model request history, not exact generated wording. File storage is an owned local fixture; encrypted storage, distributed concurrency and untrusted-state validation require a production storage design.
