$ErrorActionPreference = "Stop"
Push-Location (Join-Path $PSScriptRoot "Workspace")
try {
  dotnet restore
  if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
  dotnet build -c Release --no-restore
  if ($LASTEXITCODE -ne 0) { throw "Build failed" }
  dotnet run --project App -c Release --no-build
  if ($LASTEXITCODE -ne 0) { throw "Run failed" }
  dotnet test -c Release --no-build
  if ($LASTEXITCODE -ne 0) { throw "Test failed" }
  dotnet publish App/App.csproj -c Release --no-restore -o artifacts/app
  if ($LASTEXITCODE -ne 0) { throw "Publish failed" }
  dotnet artifacts/app/App.dll
} finally { Pop-Location }
