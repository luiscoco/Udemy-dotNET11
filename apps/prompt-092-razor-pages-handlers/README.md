# Razor Pages / page model and named post handler

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/razor-pages/
- https://learn.microsoft.com/aspnet/core/mvc/models/validation

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

The browser step uses Chrome and puppeteer-core. For a standalone download run npm install --prefix tools and set CHROME_PATH if necessary. This is a synthetic enrollment workflow; no actual enrollment is submitted to an external service.
