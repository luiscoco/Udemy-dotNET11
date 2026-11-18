# Executable preview evaluation scope

.NET SDK 11.0.100. Library, compiler and engine versions are independent. All effects use owned synthetic fixtures.

Actual experimental Blazor AI AgentBoundary, MessageList, MessageInput and AgentContext, actual AGUI.Client and a real SSE endpoint. The agent server is an explicitly deterministic protocol fixture, not a fabricated model response attributed to a live LLM. Actual browser automation clicks the Blazor button, observes streamed content/block state, captures the rendered UI, and verifies an actual RUN_ERROR failure and repair. No real external tools or consequential actions occur. Browser tests require Chrome and puppeteer-core (npm install --prefix browser-tools). Run --serve for interactive use.
