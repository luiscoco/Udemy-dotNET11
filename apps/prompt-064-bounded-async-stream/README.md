# Async streams / cancellation, bounded prefetch and disposal

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/generate-consume-asynchronous-stream
- https://learn.microsoft.com/dotnet/core/extensions/channels

The channel stores at most two queued values; the producer may also have one pending write. The iterator cancels and awaits its producer in finally when the consumer exits. The 1000-value source is finite for the materialization counterexample. Channel buffering and retained consumer results are different memory bounds.
