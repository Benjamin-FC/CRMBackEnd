# Start CRM Backend API with Real CRM Service Proxy
# Runs the API using Kestrel on localhost:5018
# Connects to actual external CRM service (UseDevelopmentCRM = false)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting CRM Backend API (Proxy Mode)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Stop any existing process on port 5018
Write-Host "Checking for existing processes on port 5018..." -ForegroundColor Yellow
$tcpConnections = Get-NetTCPConnection -LocalPort 5018 -ErrorAction SilentlyContinue
if ($tcpConnections) {
    Write-Host "Stopping existing processes..." -ForegroundColor Yellow
    $tcpConnections | ForEach-Object {
        Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 2
    Write-Host "Existing processes stopped." -ForegroundColor Green
}

Write-Host ""
Write-Host "Starting API with REAL CRM service proxy..." -ForegroundColor Yellow
Write-Host "URL: http://localhost:5018" -ForegroundColor Cyan
Write-Host "Swagger: http://localhost:5018/swagger" -ForegroundColor Cyan
Write-Host "Mode: Proxying to External CRM API" -ForegroundColor Magenta
Write-Host ""

# Check for CRM username and password
if (-not $env:CRM_USERNAME) {
    Write-Host "WARNING: CRM_USERNAME environment variable is not set!" -ForegroundColor Red
    Write-Host "Set it with: `$env:CRM_USERNAME = 'your-username'" -ForegroundColor Yellow
    Write-Host ""
    $response = Read-Host "Continue without credentials? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "Aborted." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Using CRM username from environment variable" -ForegroundColor Green
}

if (-not $env:CRM_PASSWORD) {
    Write-Host "WARNING: CRM_PASSWORD environment variable is not set!" -ForegroundColor Red
    Write-Host "Set it with: `$env:CRM_PASSWORD = 'your-password'" -ForegroundColor Yellow
    Write-Host ""
    $response = Read-Host "Continue without credentials? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Host "Aborted." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Using CRM password from environment variable" -ForegroundColor Green
}

Write-Host ""
Write-Host "Press Ctrl+C to stop the API" -ForegroundColor Yellow
Write-Host ""

# Start the API with production CRM
Set-Location $PSScriptRoot
$env:UseDevelopmentCRM = "false"
dotnet run --project src/CRMBackEnd.API/CRMBackEnd.API.csproj --urls "http://localhost:5018"
