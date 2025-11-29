# Quick Deploy to IIS
# Copies published files to IIS and restarts app pool
# Must run as Administrator

param(
    [string]$SourcePath = ".\publish",
    [string]$DestinationPath = "C:\inetpub\wwwroot\CRMBackend",
    [string]$AppPoolName = "DefaultAppPool"
)

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Quick IIS Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Copy files
Write-Host "Copying files to IIS..." -ForegroundColor Yellow
Copy-Item "$SourcePath\*" -Destination $DestinationPath -Recurse -Force
Write-Host "Files copied successfully!" -ForegroundColor Green
Write-Host ""

# Restart app pool
Write-Host "Restarting IIS App Pool..." -ForegroundColor Yellow
Import-Module WebAdministration
Restart-WebAppPool -Name $AppPoolName
Write-Host "App pool '$AppPoolName' restarted!" -ForegroundColor Green
Write-Host ""

# Wait for app to start
Write-Host "Waiting for application to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

# Test the application
Write-Host "Testing application..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost/CRMBackend/swagger" -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "Application is running successfully!" -ForegroundColor Green
        Write-Host "Swagger URL: http://localhost/CRMBackend/swagger" -ForegroundColor Cyan
    }
} catch {
    Write-Host "Warning: Could not verify application status" -ForegroundColor Yellow
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Deployment complete!" -ForegroundColor Green
