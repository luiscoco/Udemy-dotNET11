# HTTP resilience / retry budgets, timeouts and circuit breaking

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://www.pollydocs.org/strategies/retry.html
- https://www.pollydocs.org/strategies/circuit-breaker.html
- https://www.pollydocs.org/strategies/timeout.html

Pinned Polly.Core 8.8.0 needs nuget.org on first restore. All HTTP traffic targets the real local fixture. The pipeline order is total timeout, retry, breaker, attempt timeout. Retries are limited per request and by a fixed shared two-token demonstration budget; a production rolling budget needs a replenishment policy. Only GET is used, jitter is disabled for reproducible teaching, and status/exception classification is explicit. Timeouts rely on cooperative cancellation.
