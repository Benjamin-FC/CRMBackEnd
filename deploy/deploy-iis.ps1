# Deploy to IIS as Virtual Directory under Default Web Site
param(
    [string]$VirtualDirName = "CRMBackend",
    [string]$SiteName = "Default Web Site"
)

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please restart PowerShell as Administrator and try again." -ForegroundColor Yellow
    exit 1
}

# Get the root directory (parent of deploy folder)
$rootDir = Split-Path -Parent $PSScriptRoot
$publishPath = Join-Path $rootDir "publish"
$deployPath = "C:\inetpub\wwwroot\$VirtualDirName"

Write-Host "Deploying to IIS as Virtual Directory..." -ForegroundColor Cyan
Write-Host "Source: $publishPath" -ForegroundColor Gray
Write-Host "Destination: $deployPath" -ForegroundColor Gray
Write-Host "Parent Site: $SiteName" -ForegroundColor Gray
Write-Host ""

# Check if publish folder exists
if (-not (Test-Path $publishPath)) {
    Write-Host "ERROR: Publish folder not found at $publishPath" -ForegroundColor Red
    Write-Host "Please run build.ps1 first" -ForegroundColor Yellow
    exit 1
}

# Import IIS module
Import-Module WebAdministration -ErrorAction Stop

# Verify parent site exists
$site = Get-Website -Name $SiteName -ErrorAction SilentlyContinue
if (-not $site) {
    Write-Host "ERROR: Site '$SiteName' not found!" -ForegroundColor Red
    exit 1
}

Write-Host "Using application pool: $($site.applicationPool)" -ForegroundColor Gray
Write-Host ""

# Remove existing virtual directory/application if it exists
$existingVDir = Get-WebVirtualDirectory -Site $SiteName -Name $VirtualDirName -ErrorAction SilentlyContinue
$existingApp = Get-WebApplication -Site $SiteName -Name $VirtualDirName -ErrorAction SilentlyContinue

if ($existingApp) {
    Write-Host "Removing existing application '$VirtualDirName'..." -ForegroundColor Yellow
    Remove-WebApplication -Site $SiteName -Name $VirtualDirName
    Start-Sleep -Seconds 2
} elseif ($existingVDir) {
    Write-Host "Removing existing virtual directory '$VirtualDirName'..." -ForegroundColor Yellow
    Remove-WebVirtualDirectory -Site $SiteName -Name $VirtualDirName
    Start-Sleep -Seconds 2
}

# Create deployment directory
if (-not (Test-Path $deployPath)) {
    Write-Host "Creating deployment directory..." -ForegroundColor Green
    New-Item -Path $deployPath -ItemType Directory -Force | Out-Null
} else {
    Write-Host "Cleaning deployment directory..." -ForegroundColor Yellow
    Remove-Item -Path "$deployPath\*" -Recurse -Force -ErrorAction SilentlyContinue
}

# Copy files
Write-Host "Copying files to $deployPath..." -ForegroundColor Cyan
Copy-Item -Path "$publishPath\*" -Destination $deployPath -Recurse -Force

# Create application under Default Web Site (uses its app pool)
Write-Host "Creating application '$VirtualDirName' under '$SiteName'..." -ForegroundColor Green
New-WebApplication -Site $SiteName `
                   -Name $VirtualDirName `
                   -PhysicalPath $deployPath `
                   -ApplicationPool $site.applicationPool `
                   -Force | Out-Null

# Get the site binding for URL display
$binding = Get-WebBinding -Name $SiteName | Select-Object -First 1
$protocol = $binding.protocol
$port = $binding.bindingInformation.Split(':')[1]
$hostHeader = $binding.bindingInformation.Split(':')[2]

if ([string]::IsNullOrEmpty($hostHeader)) {
    $hostHeader = "localhost"
}

$baseUrl = "$protocol`://$hostHeader"
if (($protocol -eq "http" -and $port -ne "80") -or ($protocol -eq "https" -and $port -ne "443")) {
    $baseUrl += ":$port"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Application URL: $baseUrl/$VirtualDirName" -ForegroundColor Cyan
Write-Host "Swagger URL: $baseUrl/$VirtualDirName/swagger" -ForegroundColor Cyan
Write-Host "API Endpoint: $baseUrl/$VirtualDirName/api/customer/info/{id}" -ForegroundColor Cyan
Write-Host ""
Write-Host "Application Pool: $($site.applicationPool)" -ForegroundColor Gray
Write-Host "Parent Site: $SiteName" -ForegroundColor Gray
