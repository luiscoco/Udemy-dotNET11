# Child processes / arguments, captured output and cancellation

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/api/system.diagnostics.process.runandcapturetextasync?view=net-11.0
- https://devblogs.microsoft.com/dotnet/process-api-improvements-in-dotnet-11/

The application launches only its own built executable as a child. The selected .NET 11 capture helper attempts to terminate the child on cancellation. Its exception is inspected in the recording; process capture buffers output in memory, so choose a streaming reader for unbounded logs.
