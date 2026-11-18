# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

Vector uses cosine; keyword uses PostgreSQL English full-text search; hybrid uses reciprocal rank fusion with constant 60. Reranked retrieval uses supported pgvector binary quantization for a six-candidate pool and reranks original float vectors with cosine. It is a database two-stage reranker, not a neural cross-encoder. This small fixture creates HNSW and GIN indexes; it does not claim the optimizer selects an approximate index for nine rows or that one strategy always wins.
