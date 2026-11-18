# Meaningful unit tests with MSTest

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/core/testing/unit-testing-mstest-sdk

The suite runs through MSTest.Sdk 4.5.3 and Microsoft.Testing.Platform on .NET 11. It tests public pricing behavior with boundary examples, invalid inputs and rounding. The controlled regression changes the discount boundary and must fail a real test.
