@echo off
:: Deploy CRM Backend API to IIS
:: This batch file will automatically request Administrator privileges

echo CRM Backend Deployment Script
echo ================================
echo.

:: Check if running as Administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running as Administrator - proceeding with deployment...
    echo.
    powershell.exe -ExecutionPolicy Bypass -File "%~dp0deploy.ps1"
    pause
) else (
    echo Requesting Administrator privileges...
    echo.
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
)
