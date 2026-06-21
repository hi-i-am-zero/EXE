# Chay dong thoi API + Web (HTTPS)
# Dung: .\scripts\run-dev.ps1

$root = Split-Path $PSScriptRoot -Parent

Write-Host "Starting AutoWork.API  -> https://localhost:7165" -ForegroundColor Cyan
Start-Process powershell -ArgumentList @(
    '-NoExit', '-Command',
    "Set-Location '$root\src\AutoWork.API'; dotnet run --launch-profile https"
)

Start-Sleep -Seconds 3

Write-Host "Starting AutoWork.Web   -> https://localhost:7264" -ForegroundColor Cyan
Start-Process powershell -ArgumentList @(
    '-NoExit', '-Command',
    "Set-Location '$root\frontend\AutoWork.Web'; dotnet run --launch-profile https"
)

Write-Host ""
Write-Host "Done. Mo: https://localhost:7264" -ForegroundColor Green
Write-Host "API: https://localhost:7165/swagger" -ForegroundColor Green
