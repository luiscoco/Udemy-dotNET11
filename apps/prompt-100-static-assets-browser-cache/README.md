# Static assets / cache validators, browser reuse and compression

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/fundamentals/static-files
- https://learn.microsoft.com/aspnet/core/performance/caching/response

This is an isolated loopback HTTP teaching fixture. It starts Kestrel on an available local port, runs real client requests, and stops the server. It is not a production deployment or a performance benchmark.

The fixture selects Production and explicitly uses a 60-second public cache policy for its stable CSS/JS URLs. This is a teaching policy, not the framework default for every asset. Fingerprinted asset URLs are preferable for long-lived immutable production caching. Build output supplies gzip; no claim is made that a build run includes every publish-time Brotli variant. Standalone browser checks need npm install --prefix tools and a supported Chrome path. Development caching behavior differs; the supplied development setting opts into asset caching for that environment.
