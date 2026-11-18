# Compare framework-dependent, self-contained and single-file deployment

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/deploying/
- https://learn.microsoft.com/dotnet/core/deploying/single-file/overview

Windows x64 lab using .NET 11. The framework-dependent apphost uses the selected .NET 11 runtime. The self-contained and single-file outputs execute with an empty DOTNET_ROOT_X64 to verify private runtime ownership. Single-file publishing uses native-library self-extraction. Artifact reports include actual exit codes, file counts and byte sizes.
