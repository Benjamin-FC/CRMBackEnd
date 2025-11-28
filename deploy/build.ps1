# Build script for CRM Backend API
param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "publish"
)

Write-Host "Building CRM Backend API..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host ""

# Get the root directory (parent of deploy folder)
$rootDir = Split-Path -Parent $PSScriptRoot
Set-Location $rootDir

# Clean previous build
if (Test-Path $OutputPath) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Build and publish the API
Write-Host "Publishing API project..." -ForegroundColor Yellow
dotnet publish src/CRMBackEnd.API/CRMBackEnd.API.csproj `
    --configuration $Configuration `
    --output $OutputPath `
    --self-contained false `
    --runtime win-x64

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Published to: $OutputPath" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
