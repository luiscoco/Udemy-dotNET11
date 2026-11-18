# Async I/O and CPU work / choosing the scheduling boundary

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/async-scenarios

The file operation is real asynchronous file I/O plus hashing. The prime calculation is CPU work. A controlled gate demonstrates a pending task; it is not a responsiveness or throughput benchmark. Server request code does not gain CPU capacity by wrapping work in Task.Run.
