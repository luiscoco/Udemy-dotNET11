$ErrorActionPreference='Stop'
$container='dotnet-course-preview266'
$existing=docker ps -a --filter "name=^/${container}$" --format '{{.Names}}'
if($existing){
 $label=docker inspect --format '{{index .Config.Labels "course.lesson"}}' $container
 if($label -ne '266'){throw 'Container name belongs to a different workload; choose a separate course fixture.'}
 docker start $container | Out-Null
}else{
 docker run -d --name $container --label course.lesson=266 -e ACCEPT_EULA=Y -e MSSQL_PID=Developer -e 'MSSQL_SA_PASSWORD=Course266_Local!Only' -p 127.0.0.1:14666:1433 mcr.microsoft.com/mssql/server@sha256:05644dad1266ca3d445d1e8f6d2ebb9a6a03df9a39376e0f7b48917356dca9d9 | Out-Null
}
$ready=$false
for($attempt=0;$attempt -lt 30;$attempt++){
 docker exec $container /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Course266_Local!Only' -C -Q "IF DB_ID('CoursePreview266') IS NULL CREATE DATABASE CoursePreview266;" 2>$null
 if($LASTEXITCODE -eq 0){$ready=$true;break}
 Start-Sleep -Seconds 2
}
if(-not $ready){throw 'Owned SQL fixture did not become ready'}
Write-Output 'Owned loopback SQL Server ready. This is a synthetic local Developer fixture; use no production data or credentials.'
