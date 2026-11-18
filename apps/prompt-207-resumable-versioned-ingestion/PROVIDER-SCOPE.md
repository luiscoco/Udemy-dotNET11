# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

Real .NET child processes consume an actual PostgreSQL journal. The safe worker commits vector mutations and its durable checkpoint together. An actual exit code 42 before commit forces database rollback; another process resumes. The regression commits the checkpoint prematurely and skips a real version update. Documents and deletion events are trusted fixture input; source event authentication and a distributed worker lease are outside this single-worker lab.
