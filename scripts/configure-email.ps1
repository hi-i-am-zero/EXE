# Cấu hình email SMTP cho FlowMate (Gmail)
# Chạy: powershell -ExecutionPolicy Bypass -File scripts/configure-email.ps1

$ErrorActionPreference = "Stop"
$apiPath = Join-Path $PSScriptRoot "..\src\AutoWork.API"
Set-Location (Resolve-Path $apiPath)

Write-Host ""
Write-Host "=== Cấu hình Email FlowMate ===" -ForegroundColor Cyan
Write-Host "Can Gmail App Password (khong phai mat khau dang nhap thuong)."
Write-Host "Tao tai: https://myaccount.google.com/apppasswords"
Write-Host ""

$email = Read-Host "Gmail cua ban (vd: ten@gmail.com)"
$appPassword = Read-Host "Gmail App Password (16 ky tu)" -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($appPassword))

$localFile = Join-Path $apiPath "appsettings.Development.local.json"
$config = @{
    EmailSettings = @{
        SmtpHost    = "smtp.gmail.com"
        SmtpPort    = 587
        UseSsl      = $true
        Username    = $email.Trim()
        Password    = $plainPassword.Trim()
        FromEmail   = $email.Trim()
        FromName    = "FlowMate"
        AppBaseUrl  = "https://localhost:7264"
    }
} | ConvertTo-Json -Depth 3

Set-Content -Path $localFile -Value $config -Encoding UTF8

Write-Host ""
Write-Host "Da luu: $localFile" -ForegroundColor Green
Write-Host "Hay Stop debug -> Rebuild -> F5 lai API + Web." -ForegroundColor Yellow
Write-Host ""
