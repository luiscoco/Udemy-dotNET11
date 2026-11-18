# Agent provider scope

This app uses real Microsoft Agent Framework 1.28.0 agents and actual Qwen3 0.6B local CPU model inference via OllamaSharp 5.5.2 and Microsoft.Extensions.AI 11.0.0. Model text is untrusted and may vary. Exact package compatibility is verified by build and execution. Tool calling is actually exercised where claimed; deterministic adversarial inputs and human decisions are explicitly labelled fixtures.

Actual framework ApprovalRequiredAIFunction requests are approved and declined by explicitly labelled automated human-decision fixtures. A local teaching outbox is the only side effect. No real email, publication or external account is touched. A production UI must authenticate the human and protect the decision endpoint.
