# Deploy to IIS with Administrator privileges
# This script automatically elevates to Administrator and runs the full deployment

$scriptPath = $PSScriptRoot

# Check if already running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "Requesting Administrator privileges..." -ForegroundColor Yellow
    
    # Restart script with elevated permissions
    Start-Process powershell -Verb RunAs -ArgumentList "-NoExit", "-Command", "cd '$scriptPath'; .\deploy\deploy.ps1"
    
    Write-Host "Elevated window opened. This window will close in 3 seconds..." -ForegroundColor Green
    Start-Sleep -Seconds 3
    exit
}

# If already admin, run the deployment directly
Write-Host "Running with Administrator privileges" -ForegroundColor Green
Set-Location $scriptPath
& ".\deploy\deploy.ps1"
