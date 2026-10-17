@echo off
pwsh -NoProfile -File "%~dp0run-server.ps1" -DisableValidation %*
exit /b %errorlevel%
