# Agent provider scope

This app uses real Microsoft Agent Framework 1.28.0 agents and actual Qwen3 0.6B local CPU model inference via OllamaSharp 5.5.2 and Microsoft.Extensions.AI 11.0.0. Model text is untrusted and may vary. Exact package compatibility is verified by build and execution. Tool calling is actually exercised where claimed; deterministic adversarial inputs and human decisions are explicitly labelled fixtures.

The authenticated actor is a local fixture captured by the middleware instance. Each instance represents one bounded request scope, not a concurrent global rate limiter. Underlying tools still need independent resource authorization. Middleware logs identifiers and status; provider-side cancellation support remains a separate contract.
