# Agent provider scope

This app uses real Microsoft Agent Framework 1.28.0 agents and actual Qwen3 0.6B local CPU model inference via OllamaSharp 5.5.2 and Microsoft.Extensions.AI 11.0.0. Model text is untrusted and may vary. Exact package compatibility is verified by build and execution. Tool calling is actually exercised where claimed; deterministic adversarial inputs and human decisions are explicitly labelled fixtures.

The actual agent proposes typed increment actions; application code validates the allowlist, executes a local deterministic counter operation and feeds observed evidence back. Goal completion is never accepted from model claims. A no-progress tool result is an explicitly controlled fixture. Limits cover steps, model calls, local tool calls, output tokens and a cooperative deadline; this local provider has no billed API cost, and token caps are not a full monetary accounting system.
