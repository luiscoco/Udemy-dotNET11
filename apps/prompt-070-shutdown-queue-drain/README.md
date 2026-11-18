# Queue shutdown / stop admission, drain and enforce a budget

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/extensions/channels
- https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.waitasync

This in-memory queue component first closes admission, spends half its shutdown budget draining, then cancels cooperative work and joins it within the remaining budget. The fixture wait and delay observe cancellation. If arbitrary work ignores cancellation, WaitAsync can stop waiting but cannot force that code to terminate; a hard-deadline failure must be surfaced to the host. The observed milliseconds are not an OS scheduling guarantee. Abandoned work is accounted for in memory, not durably retried.
