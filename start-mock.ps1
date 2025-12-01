# Start CRM Backend API with Mock CRM Service
# Runs the API using Kestrel on localhost:5018
# Uses development/mock CRM service (UseDevelopmentCRM = true)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting CRM Backend API (Mock Mode)" -ForegroundColor Cyan
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
Write-Host "Starting API with MOCK CRM service..." -ForegroundColor Yellow
Write-Host "URL: http://localhost:5018" -ForegroundColor Cyan
Write-Host "Swagger: http://localhost:5018/swagger" -ForegroundColor Cyan
Write-Host "Mode: Using Mock/Development CRM" -ForegroundColor Green
Write-Host ""
Write-Host "Press Ctrl+C to stop the API" -ForegroundColor Yellow
Write-Host ""

# Start the API with development CRM
Set-Location $PSScriptRoot
$env:UseDevelopmentCRM = "true"
dotnet run --project src/CRMBackEnd.API/CRMBackEnd.API.csproj --urls "http://localhost:5018"
