# Agent provider scope

This app uses real Microsoft Agent Framework 1.28.0 agents and actual Qwen3 0.6B local CPU model inference via OllamaSharp 5.5.2 and Microsoft.Extensions.AI 11.0.0. Model text is untrusted and may vary. Exact package compatibility is verified by build and execution. Tool calling is actually exercised where claimed; deterministic adversarial inputs and human decisions are explicitly labelled fixtures.

Three real agent runs have independent sessions and a semaphore limits in-flight application calls to two. Each specialist selects an approved imperative checklist action from a role-specific schema, followed by independent application validation. Results are review actions, not claims that the work was completed. The local Ollama server may queue inference internally; this is not a claim of simultaneous CPU model computation or speedup. Aggregation preserves stable IDs and reports failure; agreement is not treated as proof of truth.
