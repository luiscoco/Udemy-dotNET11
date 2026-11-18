# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

Semantic boundaries use actual adjacent-sentence embedding cosine with an explicit 0.55 teaching threshold and a hard WordPiece cap. Heading chunks preserve each supplied document heading. Strategies use the same collection; counts are observations, not a claim of universal retrieval superiority. The BERT vocabulary is from sentence-transformers/all-MiniLM-L6-v2; SHA256 07eced375cec144d27c900241f3e339478dec958f92fddbc551f295c992038a3.
