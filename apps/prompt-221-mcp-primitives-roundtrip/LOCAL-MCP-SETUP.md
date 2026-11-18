# MCP lesson setup

Install .NET SDK 11.0.100 and Node.js. From this app folder, run `node verify.cjs` for build, controlled failure and repair. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to explore the actual source and process output. Dependencies are pinned in App/packages.lock.json. No real inventory, account, message or publication is changed.

Only lessons 224 and 230 require local model inference: start Ollama, run `ollama pull qwen3:0.6b`, and set `$env:COURSE_AI_ENDPOINT="http://127.0.0.1:11434"` for the default Ollama port. The recording uses port 11439. Model weights are downloaded separately. Read SCOPE.md for exact boundaries.
