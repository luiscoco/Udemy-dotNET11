$ErrorActionPreference = "Stop"
$target = Join-Path $PSScriptRoot "Workspace"
New-Item -ItemType Directory -Force -Path $target | Out-Null
dotnet new webapi -n Api -o "$target/Api" -f net11.0
if ($LASTEXITCODE -ne 0) { throw "API template creation failed" }
dotnet new blazor -n Web -o "$target/Web" -f net11.0
if ($LASTEXITCODE -ne 0) { throw "Blazor template creation failed" }
dotnet new worker -n Jobs -o "$target/Jobs" -f net11.0
if ($LASTEXITCODE -ne 0) { throw "Worker template creation failed" }
Copy-Item -LiteralPath "$PSScriptRoot/Files/Api/Program.cs" -Destination "$target/Api/Program.cs"
Copy-Item -LiteralPath "$PSScriptRoot/Files/Jobs/Program.cs" -Destination "$target/Jobs/Program.cs"
Copy-Item -LiteralPath "$PSScriptRoot/Files/Jobs/Worker.cs" -Destination "$target/Jobs/Worker.cs"
