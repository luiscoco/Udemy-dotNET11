# Agent provider scope

This app uses real Microsoft Agent Framework 1.28.0 agents and actual Qwen3 0.6B local CPU model inference via OllamaSharp 5.5.2 and Microsoft.Extensions.AI 11.0.0. Model text is untrusted and may vary. Exact package compatibility is verified by build and execution. Tool calling is actually exercised where claimed; deterministic adversarial inputs and human decisions are explicitly labelled fixtures.

An actual child process generates an agent draft, durably stores the application checkpoint, commits an independently persisted SQLite teaching outbox receipt, then exits with code 42 before checkpoint advancement. A new process resumes with the same operation ID. The outbox stands in for an external service with durable idempotency; no real mail is sent. Checkpoints are explicit application contracts, not a claimed framework checkpoint API or a general exactly-once guarantee.
