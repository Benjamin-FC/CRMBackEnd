@echo off
:: Update deployed files with Administrator privileges

echo Updating CRM Backend Deployment
echo ================================
echo.

:: Check if running as Administrator
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running as Administrator - updating files...
    echo.
    powershell.exe -ExecutionPolicy Bypass -File "%~dp0update-files.ps1"
    pause
) else (
    echo Requesting Administrator privileges...
    echo.
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
)
