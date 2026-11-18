# XML and CSV import / explicit parsing boundaries

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/api/system.xml.xmlreadersettings
- https://github.com/JoshClose/CsvHelper/tree/33.1.0

XML parsing uses framework APIs. CSV parsing uses the pinned CsvHelper 33.1.0 package and its matching API, not the rewritten version 34 API. Restore requires nuget.org access on the first run. The package lock records the dependency graph. The fixture limits input size and field and record counts; these demonstration limits should be chosen for the application.
