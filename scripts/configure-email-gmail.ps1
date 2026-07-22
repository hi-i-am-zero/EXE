# Cau hinh Gmail SMTP cho FlowMate (luu vao email.properties — team git pull la dung duoc)
# Chay: powershell -ExecutionPolicy Bypass -File scripts/configure-email-gmail.ps1
# Hoac: powershell -ExecutionPolicy Bypass -File scripts/configure-email-gmail.ps1 -Email "trinhduyet111@gmail.com" -AppPassword "abcd efgh ijkl mnop"

param(
    [string]$Email = "",
    [string]$AppPassword = ""
)

$ErrorActionPreference = "Stop"
$apiPath = Join-Path $PSScriptRoot "..\src\AutoWork.API"
Set-Location (Resolve-Path $apiPath)

Write-Host ""
Write-Host "=== Cau hinh Gmail SMTP (FlowMate) ===" -ForegroundColor Cyan
Write-Host "Can Gmail App Password: https://myaccount.google.com/apppasswords"
Write-Host "Se luu vao email.properties (commit len git de team dung chung)."
Write-Host ""

if ([string]::IsNullOrWhiteSpace($Email)) {
    $Email = Read-Host "Gmail cua ban (vd: ten@gmail.com)"
}
if ([string]::IsNullOrWhiteSpace($AppPassword)) {
    $AppPassword = Read-Host "Gmail App Password (16 ky tu, co the co khoang trang)"
}

if ([string]::IsNullOrWhiteSpace($Email) -or [string]::IsNullOrWhiteSpace($AppPassword)) {
    Write-Host "Loi: Email va App Password khong duoc de trong." -ForegroundColor Red
    exit 1
}

$Email = $Email.Trim()
$appPasswordDisplay = $AppPassword.Trim()

$propsFile = Join-Path $apiPath "email.properties"
$content = @"
# Email Configuration (team shared — git pull la dung duoc)
mail.smtp.host=smtp.gmail.com
mail.smtp.port=587
mail.smtp.auth=true
mail.smtp.starttls.enable=true
mail.smtp.ssl.enable=true
mail.smtp.ssl.trust=*

# Email Credentials
mail.username=$Email
mail.password=$appPasswordDisplay

# Sender Information
mail.from=$Email
mail.from.name=FlowMate

# Application Settings
app.base.url=https://localhost:7264

# Email Templates Settings
mail.template.encoding=UTF-8
mail.debug=false
"@

Set-Content -Path $propsFile -Value $content -Encoding UTF8

Write-Host ""
Write-Host "Da luu: $propsFile" -ForegroundColor Green
Write-Host "Commit file nay len git de ca team gui mail sau khi pull." -ForegroundColor Yellow
Write-Host "Hay Stop debug -> Rebuild -> F5 lai API + Web." -ForegroundColor Yellow
Write-Host ""
