# Prompt 11: shared build settings with InvoiceSuite

This is a runnable teaching application, built and verified using **.NET SDK 11.0.100 on Windows x64**. Checkout prints an invoice; reporting summarizes two invoices. Both reference a shared domain library. Build properties are inherited from `Directory.Build.props`, a reporting project overrides one default, and `Directory.Build.targets` logs the effective policy and emits assembly metadata read by each app.

The invoice uses a fixed 20% teaching tax rule. `TrainingRegion` is a build-metadata label; changing it does not change the tax calculation.

## Requirements and first run

Install [.NET SDK 11.0.100](https://dotnet.microsoft.com/en-us/download/dotnet/11.0) or a compatible 11.0.1xx patch permitted by `global.json`. Install Node.js to use the verification script and browser lab. No npm packages are needed for the application or lab server. No external NuGet packages are required; `NuGet.Config` clears package feeds because all reference packs come with the SDK.

From this folder:

```powershell
dotnet --version
dotnet restore InvoiceSuite.slnx
dotnet build InvoiceSuite.slnx -c Release
dotnet run --project src/Checkout -c Release --no-build
dotnet run --project src/Reporting -c Release --no-build
dotnet run --project tests/Billing.Checks -c Release --no-build
```

Expected checkout: region `EU`, net `128.90`, tax `25.78`, total `154.68`. Expected reporting: region `US`, two invoices, gross sales `454.68`. Expected checks: **8 passed**. The check runner is a console app with assertions, not an installed test framework; run it with `dotnet run`, not `dotnet test`.

## Reproduce the entire verified demonstration

```powershell
node verify.cjs
```

The script builds the solution, runs both apps, queries effective MSBuild properties, introduces a nested props file without a parent import, checks the expected compiler failure, adds the parent import, and verifies recovery. It separately introduces a possible null dereference, checks for `error CS8602`, restores the guard, rebuilds, and runs the eight behavioral checks. It restores the source in `finally` and refuses to replace an existing nested props file. Logs and `verification.json` are written to `artifacts/`.

Run this script on an unmodified exercise copy. Do not run it concurrently with the browser lab or your own edits. An interrupted process can leave generated build outputs; rerun the complete verification after restoring the exercise source.

## Interactive browser lab

```powershell
node lab-server.cjs
```

Open **http://127.0.0.1:4177**. The server listens only on the loopback interface. The source explorer reads the actual files, and action buttons launch actual `dotnet` child processes. Output streams into the terminal panel. The source view is read-only; use your editor for independent changes.

Recommended sequence:

1. Build solution, run Checkout, then Report.
2. Inspect properties; predict why reporting uses `US`.
3. Break props, inspect properties again, then Build report. Expect failure.
4. Import parent, then Build report. Expect success.
5. Reset source, Break nullable, then Build solution. Expect `CS8602`.
6. Reset source and Verify behavior. Expect a successful build and eight checks.

Reset removes only the lab's known nested props variants and restores its known nullable mutation. It refuses to remove a custom nested props file. Stop the server with Ctrl+C. You can select a different SDK executable through the `DOTNET_EXE` process environment variable.

## Files to study

- `Directory.Build.props`: early defaults and a selected warning-as-error policy.
- `Directory.Build.targets`: assembly metadata and a `BeforeTargets="CoreCompile"` hook.
- `src/Checkout/Checkout.csproj`: inherits the defaults and references the domain library.
- `src/Reporting/Reporting.csproj`: overrides the region after the early props import.
- `src/Billing.Core/Invoice.cs`: calculations, validation, snapshot behavior, and explicit null handling.
- `tests/Billing.Checks/Program.cs`: eight checks with expected business results and rejected inputs.
- `EXERCISE.txt`: independent practice.

## Recording and evidence

The accompanying pilot video uses Puppeteer to operate this browser lab, with real process output rather than invented terminal screens. Unchanged waits are shortened for teaching; original command durations and full output are retained in the execution evidence. A focused diagnostic view displays exact selected lines from the full compiler log. The source, expected failures, and repaired builds were executed; visual/media validation alone is not used as evidence of application correctness.

Build logs can include local paths. The included binary log is from this controlled, dependency-free exercise. The SDK itself and generated `bin`/`obj` directories are not part of the source archive.

## Primary references

- [Customize the build by folder](https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory)
- [MSBuild command-line reference](https://learn.microsoft.com/visualstudio/msbuild/msbuild-command-line-reference)
- [.NET 11 SDK download](https://dotnet.microsoft.com/en-us/download/dotnet/11.0)
