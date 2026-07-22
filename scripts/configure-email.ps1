# Menu cau hinh email FlowMate
# CHAY DUNG LENH (khong copy noi dung script vao terminal):
#   powershell -ExecutionPolicy Bypass -File scripts/configure-email.ps1

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=== Cau hinh Email FlowMate ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "  1) Gmail SMTP"
Write-Host "  2) Brevo API (khuyen dung)"
Write-Host ""
$choice = Read-Host "Chon (1 hoac 2)"

$scriptDir = $PSScriptRoot
if ($choice -eq "2") {
    & (Join-Path $scriptDir "configure-email-brevo.ps1")
}
elseif ($choice -eq "1") {
    & (Join-Path $scriptDir "configure-email-gmail.ps1")
}
else {
    Write-Host "Loi: Chi chon 1 hoac 2." -ForegroundColor Red
    exit 1
}
