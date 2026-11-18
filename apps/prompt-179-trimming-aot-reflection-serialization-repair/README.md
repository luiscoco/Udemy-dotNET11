# Resolve trimming and AOT compatibility issues

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/deploying/trimming/prepare-libraries-for-trimming
- https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/source-generation

Windows x64 with Native AOT C++ prerequisites. Unsafe is a deliberate warning probe. The repaired implementation uses a closed registry, a normal delegate and source-generated JSON metadata. Its native executable is actually published and run. IL2026 and IL3050 diagnostics are retained. The repair covers the demonstrated closed set of course DTOs.
