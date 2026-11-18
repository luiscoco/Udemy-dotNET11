# Agent lesson setup

Use .NET SDK 11.0.100, Node.js and Ollama. Run `ollama pull qwen3:0.6b`, then configure COURSE_AI_ENDPOINT (the recording uses http://127.0.0.1:11439; the default Ollama port is 11434). Execute `node verify.cjs` for build, controlled failure and repair, or `node lab-server.cjs` for the interactive source lab.

Microsoft.Agents.AI 1.28.0 and the exact resolved dependencies are pinned in App/packages.lock.json. This independent SDK is not part of the .NET 11 runtime. The local model supports tool calling; actual recorded model digests are in MODEL-PROVENANCE.json. Model weights are downloaded separately. All identities, policies and side effects are authored teaching fixtures. Read PROVIDER-SCOPE.md.

AgentRuntime.cs is the active model factory for this lesson. The unused AiRuntime.cs file is retained shared scaffolding; it is not called by the agent application.

## PowerShell quick start

Ensure Ollama is serving in another terminal (`ollama serve`), or use an already running Ollama desktop service. From this application folder:

```powershell
ollama pull qwen3:0.6b
$env:COURSE_AI_ENDPOINT="http://127.0.0.1:11434"
node verify.cjs
node lab-server.cjs
```

Open http://127.0.0.1:4177 for the source lab. Use your actual Ollama port if it differs from 11434.
