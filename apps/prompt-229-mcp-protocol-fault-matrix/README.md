# MCP malformed requests, disconnects and capabilities

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the supplied exercise files when it finishes.

- https://github.com/modelcontextprotocol/csharp-sdk/tree/64cfd66b147553d8cf953f9e2a374aae96014968
- https://modelcontextprotocol.io/specification/2025-11-25

Uses .NET SDK 11.0.100 and official ModelContextProtocol SDK 2.4.0, with MCP protocol 2025-11-25 explicitly pinned. Endpoints and data are owned loopback teaching fixtures. See LOCAL-MCP-SETUP.md and SCOPE.md.

