# Bounded channels / backpressure, workers and completion

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/extensions/channels

The no-loss fixture uses the default Wait full mode. Two readers may finish work out of input order, so results are sorted only for verification. Shared cancellation releases the writer after a consumer failure, and the consumer tasks are awaited. The exactly-once assertion describes this in-process run; an in-memory channel is not a durable or distributed delivery guarantee.
