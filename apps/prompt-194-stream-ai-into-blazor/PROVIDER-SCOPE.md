# Local inference scope

The recording uses Ollama 0.40.16 and Qwen2.5 0.5B, with pinned .NET packages in App.csproj and packages.lock.json. Requests execute actual model inference; model wording is not asserted. Deterministic safety assertions inspect application boundaries and controlled fixtures explicitly. Tool authorization and user isolation are enforced by application code, independently of model compliance. Model output is untrusted.

This lab starts actual ASP.NET Core Kestrel, a server-interactive Blazor component and Chromium. The model endpoint writes NDJSON updates from actual Ollama inference. The browser automation records component updates and a completed screenshot. The controlled regression buffers all provider updates into one line; it does not invent model content. HTTP byte boundaries are not assumed to equal model tokens. The localhost teaching endpoint has no production authentication policy.
