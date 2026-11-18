$ErrorActionPreference = "Stop"
$target = Join-Path $PSScriptRoot "Workspace"
New-Item -ItemType Directory -Force -Path $target | Out-Null
Push-Location $target
try {
  dotnet new sln -n MyApp
  dotnet new webapi -n Api -o src/Api -f net11.0
  if ($LASTEXITCODE -ne 0) { throw "API template creation failed" }
  dotnet new xunit -n Api.Tests -o tests/Api.Tests -f net11.0
  if ($LASTEXITCODE -ne 0) { throw "Test template creation failed" }
  dotnet sln add src/Api/Api.csproj tests/Api.Tests/Api.Tests.csproj
  dotnet add tests/Api.Tests/Api.Tests.csproj reference src/Api/Api.csproj
  dotnet new gitignore
  Copy-Item -Path "$PSScriptRoot/Files/Api/*.cs" -Destination src/Api
  Copy-Item -Path "$PSScriptRoot/Files/Api.Tests/*.cs" -Destination tests/Api.Tests
  dotnet user-secrets init --project src/Api
  dotnet user-secrets set "Demo:ApiKey" "local-demo-value" --project src/Api
} finally { Pop-Location }
