# Foundry Local inference in an offline worker

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/machine-learning/

Uses .NET SDK 11.0.100 with separately pinned model packages. All business data and side effects are synthetic teaching fixtures. Read SCOPE.md for model, hardware, data and provider requirements.
Windows x64 is required for the AppContainer demonstration. Acquire the compatible Foundry model with the supplied prepare-model project, then set COURSE_FOUNDRY_MODEL_PATH to its actual model directory. Model weights are not bundled. The verifier publishes a self-contained worker, gives a temporary capability-less AppContainer read access only to its executable/model directories and write access to its owned result directory, and removes those access entries and profile afterward. Host networking is not disabled.

