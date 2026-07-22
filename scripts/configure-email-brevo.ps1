# Cau hinh Brevo API cho FlowMate
# Chay: powershell -ExecutionPolicy Bypass -File scripts/configure-email-brevo.ps1

$ErrorActionPreference = "Stop"
$apiPath = Join-Path $PSScriptRoot "..\src\AutoWork.API"
Set-Location (Resolve-Path $apiPath)

Write-Host ""
Write-Host "=== Cau hinh Brevo (FlowMate) ===" -ForegroundColor Cyan
Write-Host "Lay API key: https://app.brevo.com/settings/keys/api"
Write-Host "Xac minh sender: https://app.brevo.com/senders"
Write-Host ""

$fromEmail = Read-Host "Email nguoi gui da xac minh tren Brevo (vd: ten@gmail.com)"
$apiKeyPlain = Read-Host "Brevo API Key (xkeysib-...)"

if ([string]::IsNullOrWhiteSpace($fromEmail) -or [string]::IsNullOrWhiteSpace($apiKeyPlain)) {
    Write-Host "Loi: Email va API key khong duoc de trong." -ForegroundColor Red
    exit 1
}

$fromEmail = $fromEmail.Trim()
$apiKeyPlain = $apiKeyPlain.Trim()

$config = @{
    EmailSettings = @{
        Provider           = "Brevo"
        BrevoApiKey        = $apiKeyPlain
        FromEmail          = $fromEmail
        FromName           = "FlowMate"
        AppBaseUrl         = "https://localhost:7264"
        UseDevFileFallback = $true
    }
}

$localFile = Join-Path $apiPath "appsettings.Development.local.json"
$config | ConvertTo-Json -Depth 3 | Set-Content -Path $localFile -Encoding UTF8

dotnet user-secrets set "EmailSettings:Provider" "Brevo" | Out-Null
dotnet user-secrets set "EmailSettings:BrevoApiKey" $apiKeyPlain | Out-Null
dotnet user-secrets set "EmailSettings:FromEmail" $fromEmail | Out-Null
dotnet user-secrets set "EmailSettings:AppBaseUrl" "https://localhost:7264" | Out-Null

Write-Host ""
Write-Host "Da luu: $localFile" -ForegroundColor Green
Write-Host "Hay Stop debug -> Rebuild -> F5 lai API + Web." -ForegroundColor Yellow
Write-Host ""
