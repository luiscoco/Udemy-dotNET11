# Playwright journeys with authentication and validation failures

This exercise runs on .NET SDK 11.0.100 or a compatible patch and uses the supplied source. Install Node.js for the browser lab and verification runner.

Run `node verify.cjs` to execute the checks and controlled experiments. Run `node lab-server.cjs` and open http://127.0.0.1:4177 to inspect source and run the same commands interactively. The action buttons execute real .NET processes. Read EXERCISE.txt and preserve your own edits before resetting the exercise.

The verification report and command output are written to artifacts/verification.json. The verifier restores the controlled compiler probe when it finishes.

- https://playwright.dev/dotnet/docs/writing-tests
- https://learn.microsoft.com/aspnet/core/security/anti-request-forgery

Microsoft.Playwright drives the installed Chrome browser against a real loopback Kestrel application. Set CHROME_PATH when Chrome uses another path. The demonstration account is synthetic. The application uses real ASP.NET Core cookie authentication and antiforgery tokens with ephemeral data-protection keys. The journey verifies denied access, rejected credentials, server validation and a successful save. Production authentication requires its own identity and credential storage design.
