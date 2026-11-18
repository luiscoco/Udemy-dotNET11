# Produce reproducible container images with the .NET 11 SDK

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/whats-new/dotnet-11/sdk
- https://learn.microsoft.com/dotnet/core/containers/publish-configuration

The SDK performs real container archive publishing for linux-x64. SOURCE_DATE_EPOCH uses the fixed fixture source timestamp. Base image, project inputs, version and image format are pinned. The manifest digest is captured from the SDK GeneratedContainerDigest property. The comparison covers these stated inputs and this selected SDK. Container archive binaries are rebuilt by the exercise.
