# OpenAPI and response contract verification

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview
- https://learn.microsoft.com/aspnet/core/test/integration-tests

The tests request the actual generated OpenAPI document and actual JSON responses through WebApplicationFactory. A checked-in baseline covers the demonstrated course properties, required fields and types. Unreviewed additive changes also require baseline review under this intentionally strict example policy. The suite covers the displayed endpoint contract. Broader API coverage requires additional operations and compatibility rules.
