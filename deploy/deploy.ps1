# Full deployment script - Build and Deploy to IIS
param(
    [string]$Configuration = "Release",
    [string]$VirtualDirName = "CRMBackend",
    [string]$SiteName = "Default Web Site"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CRM Backend API - Full Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build
Write-Host "Step 1: Building application..." -ForegroundColor Cyan
& "$PSScriptRoot\build.ps1" -Configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Aborting deployment." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 2: Deploying to IIS..." -ForegroundColor Cyan

# Step 2: Deploy
& "$PSScriptRoot\deploy-iis.ps1" -VirtualDirName $VirtualDirName -SiteName $SiteName
if ($LASTEXITCODE -ne 0) {
    Write-Host "Deployment failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Deployment process completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
