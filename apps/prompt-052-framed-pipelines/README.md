# Pipelines / fragmented length-prefixed messages

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/standard/io/pipelines

System.IO.Pipelines is supplied through the Microsoft.AspNetCore.App shared framework reference. Install the .NET 11 SDK, which includes that shared framework. The receiver retains a list only for this two-message fixture; a production receiver should dispatch each bounded frame to its consumer.
