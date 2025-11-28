# Update files in IIS deployment (requires Administrator)

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

$rootDir = Split-Path -Parent $PSScriptRoot
$publishPath = Join-Path $rootDir "publish"
$deployPath = "C:\inetpub\wwwroot\CRMBackend"

Write-Host "Updating deployed files..." -ForegroundColor Cyan
Write-Host "Source: $publishPath" -ForegroundColor Gray
Write-Host "Destination: $deployPath" -ForegroundColor Gray
Write-Host ""

# Stop the app pool temporarily to release file locks
Import-Module WebAdministration -ErrorAction Stop
$site = Get-Website -Name "Default Web Site" -ErrorAction SilentlyContinue
if ($site) {
    $appPool = $site.applicationPool
    Write-Host "Stopping app pool: $appPool" -ForegroundColor Yellow
    Stop-WebAppPool -Name $appPool -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

# Copy files
Write-Host "Copying files..." -ForegroundColor Cyan
Copy-Item -Path "$publishPath\*" -Destination $deployPath -Recurse -Force

# Start the app pool
if ($site) {
    Write-Host "Starting app pool: $appPool" -ForegroundColor Green
    Start-WebAppPool -Name $appPool
}

Write-Host ""
Write-Host "Files updated successfully!" -ForegroundColor Green
Write-Host "Application URL: http://localhost/CRMBackend" -ForegroundColor Cyan
Write-Host "Swagger URL: http://localhost/CRMBackend/swagger" -ForegroundColor Cyan
