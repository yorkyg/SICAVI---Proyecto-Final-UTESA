@echo off
echo Verificando SDK de .NET...
where dotnet >nul 2>nul
if errorlevel 1 (
  echo NO se encontro dotnet. Instala el SDK de .NET 8 desde Visual Studio Installer.
  pause
  exit /b 1
)

dotnet --list-sdks
echo.
echo Debe aparecer una version 8.0 o superior.
pause
