# Switch actual model providers through configuration

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/ai/ichatclient
- https://github.com/awaescher/OllamaSharp
- https://docs.ollama.com/api/chat

Install Ollama and run `ollama serve`, then `ollama pull qwen2.5:0.5b`. Set COURSE_AI_ENDPOINT to your local Ollama URL (default http://127.0.0.1:11439 for the recording; standard Ollama installations normally use port 11434). This sample performs actual local CPU model inference. No paid model API key is required. First download requires internet access and sufficient RAM. The small teaching model is not a production quality recommendation. Read PROVIDER-SCOPE.md for limitations.



## Native model path

After `ollama pull qwen2.5:0.5b`, the Windows default weights path is `%USERPROFILE%\.ollama\models\blobs\sha256-c5396e06af294bd101b30dce59131a76d2b773e76950acc870eda801d3ab0515`. Set COURSE_AI_GGUF to that file, or to the corresponding blob in your configured OLLAMA_MODELS directory. On other systems use the same model blob from your Ollama data directory. The native engine needs its CPU backend package restored and enough free memory to load the model.

Follow [LOCAL-AI-SETUP.md](LOCAL-AI-SETUP.md) for exact provider, model-path and browser commands.
