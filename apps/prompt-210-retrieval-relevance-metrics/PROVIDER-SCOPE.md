# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

The five questions and graded relevance judgments are authored fixture labels. Unlisted documents are treated as nonrelevant for this small closed collection; unjudged positive-free questions are rejected. Reported metrics are measured on actual retrieval results and are not a general benchmark. Independent known-ranking probes verify the recall denominator, reciprocal rank and logarithmic discount.
