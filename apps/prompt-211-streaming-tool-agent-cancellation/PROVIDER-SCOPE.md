# Agent provider scope

This app uses real Microsoft Agent Framework 1.28.0 agents and actual Qwen3 0.6B local CPU model inference via OllamaSharp 5.5.2 and Microsoft.Extensions.AI 11.0.0. Model text is untrusted and may vary. Exact package compatibility is verified by build and execution. Tool calling is actually exercised where claimed; deterministic adversarial inputs and human decisions are explicitly labelled fixtures.

The read-only GetCourse tool uses authored synthetic catalog data. The local model actually selects the function and the framework invokes it. Cancellation is requested after actual text starts arriving, and an outer deadline remains in effect even in the controlled failure.
