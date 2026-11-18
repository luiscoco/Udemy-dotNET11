# Build CI gates and versioned deployment artifacts

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/devops/dotnet-cli-and-continuous-integration
- https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-sdk

The GitHub Actions workflow is supplied for review and runs the same pipeline driver. This lesson executes its gates locally. It restores checked-in lock files, builds with warnings as errors, executes three actual MSTest cases and creates a versioned win-x64 artifact with SHA256 hashes. The fixture restores its controlled source edit. No remote repository or hosted workflow is changed.
