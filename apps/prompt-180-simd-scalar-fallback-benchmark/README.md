# Benchmark SIMD with a correct scalar fallback

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/standard/simd
- https://learn.microsoft.com/dotnet/api/system.numerics.vector-1

Vector<int> uses the current host supported vector width. Integer results use unchecked wraparound consistently. Timings are local observations and correctness checks cover vector boundaries. The fallback can be explicitly selected and also applies when hardware acceleration is absent.
