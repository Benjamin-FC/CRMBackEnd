@echo off
REM Start CRM Backend API in local development mode

echo ========================================
echo Starting CRM Backend API (Local Mode)
echo ========================================
echo.
echo URL: http://localhost:5018
echo Swagger: http://localhost:5018/swagger
echo.
echo Press Ctrl+C to stop the API
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0start-local.ps1"

pause
