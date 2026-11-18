# Passing suite and reports
dotnet test --solution Course.slnx -c Release --no-build --filter TestCategory=Passing --results-directory artifacts/repair --results-directory-layout per-module --report-trx --report-trx-filename results.trx

# Deliberate timeout probe
dotnet test --project ModuleA/ModuleA.csproj -c Release --no-build --filter TestCategory=Slow --timeout 5s

# Deliberate failure-limit probe
dotnet test --project ModuleA/ModuleA.csproj -c Release --no-build --filter TestCategory=Failing --maximum-failed-tests 1
