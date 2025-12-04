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

# Validate all required environment variables
$missingVars = @()

if (-not $env:CRM_USERNAME) { $missingVars += "CRM_USERNAME" }
if (-not $env:CRM_PASSWORD) { $missingVars += "CRM_PASSWORD" }
if (-not $env:CRM_CLIENT_ID) { $missingVars += "CRM_CLIENT_ID" }
if (-not $env:CRM_CLIENT_SECRET) { $missingVars += "CRM_CLIENT_SECRET" }
if (-not $env:CRM_TOKEN_URL) { $missingVars += "CRM_TOKEN_URL" }
if (-not $env:CRM_SCOPE) { $missingVars += "CRM_SCOPE" }

if ($missingVars.Count -gt 0) {
    Write-Host "ERROR: Required environment variables are missing!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Missing variables:" -ForegroundColor Yellow
    foreach ($var in $missingVars) {
        Write-Host "  - $var" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Run the credential setup script first:" -ForegroundColor Cyan
    Write-Host "  .\set-crm-credentials.ps1" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "All required environment variables are set:" -ForegroundColor Green
Write-Host "  √ CRM_USERNAME" -ForegroundColor Green
Write-Host "  √ CRM_PASSWORD" -ForegroundColor Green
Write-Host "  √ CRM_CLIENT_ID" -ForegroundColor Green
Write-Host "  √ CRM_CLIENT_SECRET" -ForegroundColor Green
Write-Host "  √ CRM_TOKEN_URL" -ForegroundColor Green
Write-Host "  √ CRM_SCOPE" -ForegroundColor Green

Write-Host ""
Write-Host "Press Ctrl+C to stop the API" -ForegroundColor Yellow
Write-Host ""

# Start the API with production CRM
Set-Location $PSScriptRoot
$env:UseDevelopmentCRM = "false"
dotnet run --project src/CRMBackEnd.API/CRMBackEnd.API.csproj --urls "http://localhost:5018"
