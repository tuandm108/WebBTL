@echo off
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup-local-mqtt.ps1" %*
exit /b %errorlevel%
