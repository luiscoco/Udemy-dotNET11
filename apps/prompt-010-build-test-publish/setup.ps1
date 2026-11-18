$ErrorActionPreference = "Stop"
$target = Join-Path $PSScriptRoot "Workspace"
New-Item -ItemType Directory -Force -Path $target | Out-Null
Push-Location $target
try {
  dotnet new sln -n Flow
  dotnet new console -n App -f net11.0
  if ($LASTEXITCODE -ne 0) { throw "Console template creation failed" }
  dotnet new xunit -n App.Tests -f net11.0
  if ($LASTEXITCODE -ne 0) { throw "Test template creation failed" }
  dotnet sln add App/App.csproj App.Tests/App.Tests.csproj
  dotnet add App.Tests/App.Tests.csproj reference App/App.csproj
  Copy-Item -Path "$PSScriptRoot/Files/App/*.cs" -Destination App
  Copy-Item -Path "$PSScriptRoot/Files/App.Tests/*.cs" -Destination App.Tests
} finally { Pop-Location }
