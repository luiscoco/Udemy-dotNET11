# Run the local AI lessons

Install .NET SDK 11.0.100 (or a compatible patch allowed by global.json), Node.js 24 and Ollama. Download the model separately; weights and provider runtime binaries are not included in the source ZIP.

In one terminal start the provider:

```powershell
ollama serve
```

In another PowerShell terminal, from this lesson folder:

```powershell
ollama pull qwen2.5:0.5b
$env:COURSE_AI_ENDPOINT='http://127.0.0.1:11434'
node verify.cjs
```

If Ollama is already running, use that service instead of starting another. The recording uses an isolated loopback service on port 11439; ordinary Ollama defaults to 11434. The endpoint setting must match your service. Linux/macOS shells can use `export COURSE_AI_ENDPOINT=http://127.0.0.1:11434`.

For lessons 192 and 200, set the native model path before verification. For the recorded Qwen weights in the standard Windows Ollama data folder:

```powershell
$env:COURSE_AI_GGUF=Join-Path $env:USERPROFILE '.ollama/models/blobs/sha256-c5396e06af294bd101b30dce59131a76d2b773e76950acc870eda801d3ab0515'
```

If your OLLAMA_MODELS folder differs, use its corresponding model blob. MODEL-PROVENANCE.json records the tested identity; PROVIDER-SCOPE.md explains the adapter and model limits. Native inference needs enough available RAM and the restored CPU backend package.

Lesson 194 additionally needs Chrome. Install its browser driver dependency with `npm install --prefix tools`. CHROME_PATH can identify your Chrome executable when it is not at the standard Windows path. Run `dotnet run --project App -c Release -- serve` to leave the interactive Blazor page running, then open the printed address. The browser tests use their own headless profile and never operate your normal Chrome tabs.

Run `node lab-server.cjs` to use the source-and-process teaching interface at http://127.0.0.1:4177. Its buttons execute the same real .NET commands used by the verifier.
