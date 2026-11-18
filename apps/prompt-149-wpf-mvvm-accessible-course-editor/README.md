# WPF MVVM, resources and accessible controls

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/dotnet/desktop/wpf/data/
- https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/

This application targets Windows x64 using WPF on .NET 11. The App project launches the actual Native application. Verification uses its real control and binding paths, records assertions, and renders its actual visual tree to PNG. Launch NativeCourse.exe without COURSE_MODE for normal interactive use.
