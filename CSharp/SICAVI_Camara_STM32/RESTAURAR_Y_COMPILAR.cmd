@echo off
setlocal
cd /d "%~dp0"

echo ================================================
echo SICAVI - Restauracion y compilacion de paquetes
echo ================================================
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: No se encontro el SDK de .NET.
    echo Instala .NET 8 SDK y la carga Desarrollo de escritorio de .NET.
    pause
    exit /b 1
)

dotnet --version
echo.
echo Restaurando OpenCvSharp y System.IO.Ports...
dotnet restore "SICAVI.sln" --configfile "NuGet.Config" --force --no-cache --verbosity normal

if errorlevel 1 (
    echo.
    echo LA RESTAURACION FALLO.
    echo Si aparece NU1301 o api.nuget.org, el problema es Internet, DNS, proxy o firewall.
    echo Prueba abrir https://api.nuget.org/v3/index.json en el navegador.
    echo Tambien puedes probar temporalmente con el punto de acceso del telefono.
    echo.
    nslookup api.nuget.org
    pause
    exit /b 1
)

echo.
echo Compilando SICAVI...
dotnet build "SICAVI.sln" -c Debug --no-restore

if errorlevel 1 (
    echo.
    echo La restauracion termino, pero la compilacion encontro errores.
    echo Envia una captura de esta ventana desde el primer error mostrado.
    pause
    exit /b 1
)

echo.
echo COMPILACION CORRECTA.
echo Ya puedes abrir SICAVI.sln y ejecutar con F5.
pause
exit /b 0
