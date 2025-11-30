# Start CRM Backend API in local development mode
# Runs the API using Kestrel on localhost:5018

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting CRM Backend API (Local Mode)" -ForegroundColor Cyan
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
Write-Host "Starting API..." -ForegroundColor Yellow
Write-Host "URL: http://localhost:5018" -ForegroundColor Cyan
Write-Host "Swagger: http://localhost:5018/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the API" -ForegroundColor Yellow
Write-Host ""

# Start the API
Set-Location $PSScriptRoot
dotnet run --project src/CRMBackEnd.API/CRMBackEnd.API.csproj --urls "http://localhost:5018"
