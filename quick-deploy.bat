@echo off
REM Quick Deploy - Copy files and restart IIS
REM Automatically elevates to Administrator

net session >nul 2>&1
if %errorLevel% == 0 (
    REM Already admin, run quick deploy
    powershell -ExecutionPolicy Bypass -File "%~dp0deploy\quick-deploy.ps1"
) else (
    REM Request elevation
    echo Requesting Administrator privileges...
    powershell -Command "Start-Process powershell -Verb RunAs -ArgumentList '-NoExit', '-ExecutionPolicy', 'Bypass', '-File', '%~dp0deploy\quick-deploy.ps1'"
)

pause
