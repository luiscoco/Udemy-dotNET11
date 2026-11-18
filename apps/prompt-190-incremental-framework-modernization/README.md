# Modernize a .NET Framework application incrementally

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://learn.microsoft.com/aspnet/core/migration/fx-to-core/
- https://learn.microsoft.com/dotnet/core/porting/framework-overview

Windows with .NET Framework 4.8 and .NET 11. The SDK-style legacy adapter compiles against pinned reference assemblies and executes on the installed Framework runtime. System.Web and System.Web.Extensions remain behind the process boundary. Actual loopback HTTP checks compare the modern invoice with legacy output and preserve the legacy report route. The isolated dependency probe intentionally fails compilation on net11.0.
