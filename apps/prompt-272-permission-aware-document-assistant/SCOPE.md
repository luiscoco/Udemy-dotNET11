# Executable capstone scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Real Markdown ingestion into SQLite, exact tenant/subject permission filtering before context construction, actual local Qwen/Ollama generation streamed through authenticated HTTP NDJSON, and application-attached citations with source hashes. Retrieval is deliberately bounded and deterministic, not neural vector ranking. Four permission evaluations run independently of model wording. Requires Ollama at COURSE_AI_ENDPOINT or localhost:11439 and qwen2.5:0.5b. Model answers remain untrusted and citations do not prove semantic correctness.
