# MAUI Windows platform foundation

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/maui/

This lesson selects Windows x64 on .NET MAUI 11.0.0. Install the .NET 11 maui-windows workload and the Windows App SDK runtime required by the restored packages. The App project launches the actual compiled Native application and reads its verification report. Native PNG evidence is rendered from the application’s WinUI controls. Launch NativeCourse.exe without COURSE_MODE to use the normal interactive application. The local exercise does not deploy to Android, iOS or Mac Catalyst.


