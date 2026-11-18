# Test injections in documents and tool responses

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/ai/evaluation/libraries
- https://learn.microsoft.com/dotnet/ai/evaluation/evaluate-ai-response

Runs on .NET SDK 11.0.100. Data and identities are synthetic teaching fixtures. No production service is modified. Read SCOPE.md.
Requires actual Ollama inference: ollama pull qwen3:0.6b; set COURSE_AI_ENDPOINT to your running server (normally http://127.0.0.1:11434). The recording uses loopback port 11439. Model weights are downloaded separately.

