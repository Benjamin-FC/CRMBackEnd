# Undeploy/Remove CRM Backend API from IIS Virtual Directory
param(
    [string]$VirtualDirName = "CRMBackend",
    [string]$SiteName = "Default Web Site",
    [switch]$RemoveFiles = $false
)

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please restart PowerShell as Administrator and try again." -ForegroundColor Yellow
    exit 1
}

$deployPath = "C:\inetpub\wwwroot\$VirtualDirName"

Write-Host "Undeploying from IIS..." -ForegroundColor Cyan
Write-Host "Parent Site: $SiteName" -ForegroundColor Gray
Write-Host "Virtual Directory: $VirtualDirName" -ForegroundColor Gray
Write-Host ""

# Import IIS module
Import-Module WebAdministration -ErrorAction Stop

# Remove application or virtual directory
$existingApp = Get-WebApplication -Site $SiteName -Name $VirtualDirName -ErrorAction SilentlyContinue
$existingVDir = Get-WebVirtualDirectory -Site $SiteName -Name $VirtualDirName -ErrorAction SilentlyContinue

if ($existingApp) {
    Write-Host "Removing application '$VirtualDirName' from '$SiteName'..." -ForegroundColor Yellow
    Remove-WebApplication -Site $SiteName -Name $VirtualDirName
    Write-Host "Application removed successfully" -ForegroundColor Green
} elseif ($existingVDir) {
    Write-Host "Removing virtual directory '$VirtualDirName' from '$SiteName'..." -ForegroundColor Yellow
    Remove-WebVirtualDirectory -Site $SiteName -Name $VirtualDirName
    Write-Host "Virtual directory removed successfully" -ForegroundColor Green
} else {
    Write-Host "Application/Virtual directory '$VirtualDirName' not found under '$SiteName'" -ForegroundColor Yellow
}

# Remove files if requested
if ($RemoveFiles -and (Test-Path $deployPath)) {
    Write-Host "Removing deployment files from $deployPath..." -ForegroundColor Yellow
    Start-Sleep -Seconds 2
    Remove-Item -Path $deployPath -Recurse -Force
    Write-Host "Files removed successfully" -ForegroundColor Green
}

Write-Host ""
Write-Host "Undeployment completed!" -ForegroundColor Green
