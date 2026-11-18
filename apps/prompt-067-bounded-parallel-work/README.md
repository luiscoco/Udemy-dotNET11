# Bounded parallelism / throughput and in-flight resources

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/api/system.threading.tasks.parallel.foreachasync
- https://learn.microsoft.com/dotnet/api/system.threading.tasks.paralleloptions.maxdegreeofparallelism

The workload contains a controlled 10 ms asynchronous wait and a 4096-byte buffer per active operation. It measures fixture throughput and peak owned in-flight buffers, not total process memory, CPU parallel speedup or production network behavior. No assertion requires one measured timing to beat another.
