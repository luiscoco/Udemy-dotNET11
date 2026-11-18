# Wire formats / JSON, CBOR and MessagePack contracts

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/api/system.formats.cbor.cborreader
- https://github.com/MessagePack-CSharp/MessagePack-CSharp/tree/v3.1.12

System.Text.Json is in the framework. System.Formats.Cbor 11.0.0 and external MessagePack 3.1.12 are explicit pinned packages; the first restore needs nuget.org. The lock file records their dependency graph. UntrustedData mode adds MessagePack resource defenses, not authentication. This small bounded telemetry example compares wire size and round-trip behavior, not general throughput or security guarantees.
