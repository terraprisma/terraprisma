@echo off

if exist "%~dp0dotnet\dotnet.exe" (
    echo using local dotnet
    "%~dp0dotnet\dotnet.exe" %*
) else (
    dotnet %*
)
