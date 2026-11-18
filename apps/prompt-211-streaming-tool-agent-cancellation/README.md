# Streaming agent with tools and cancellation

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the supplied exercise files when it finishes.

- https://learn.microsoft.com/agent-framework/
- https://github.com/microsoft/agent-framework/tree/36265445e6f64dbd4d3566f9f9e940e7a2d6e8d3/dotnet/samples

Install Ollama and run `ollama serve`, then `ollama pull qwen3:0.6b`. Set COURSE_AI_ENDPOINT to your local Ollama URL (default http://127.0.0.1:11439 for the recording; standard Ollama installations normally use port 11434). This sample performs actual local CPU model inference. No paid model API key is required. First download requires internet access and sufficient RAM. The small teaching model is not a production quality recommendation. Read PROVIDER-SCOPE.md for limitations.

Run Ollama on the configured loopback endpoint and pull qwen3:0.6b. See LOCAL-AGENT-SETUP.md. This lab uses actual Microsoft Agent Framework agents and local CPU inference.

