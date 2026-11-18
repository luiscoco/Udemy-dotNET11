# Static interface helpers / older runtime compatibility

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/interface

The legacy executable demonstration requires Windows with 64-bit .NET Framework 4 installed. The .NET 11 harness locates the SDK compiler through DOTNET_ROOT or the default Program Files installation. Set DOTNET_EXE to an absolute dotnet.exe path when using a custom SDK installation. The compiler references the installed Framework core library and launches the resulting CLR 4 executable directly.
