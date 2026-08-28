# Solucion de errores Mat, Rect, VideoCapture y SerialPort

Estos mensajes no representan decenas de fallos diferentes. Aparecen cuando
Visual Studio no ha restaurado las dependencias del proyecto.

El archivo `Sicavi.WinForms.csproj` incluye:

- `OpenCvSharp4.Windows`: Mat, Rect, Scalar, VideoCapture y Cv2.
- `OpenCvSharp4.Extensions`: BitmapConverter.
- `System.IO.Ports`: SerialPort, Parity, StopBits, Handshake y eventos seriales.

## Procedimiento automatico

1. Cierra Visual Studio.
2. Verifica que la computadora tenga Internet.
3. Ejecuta `RESTAURAR_Y_COMPILAR.cmd`.
4. Espera el mensaje `COMPILACION CORRECTA`.
5. Abre `SICAVI.sln` y ejecuta con F5.

## Si aparece NU1301

Abre esta direccion en el navegador:

`https://api.nuget.org/v3/index.json`

Si no abre, Windows no puede comunicarse con NuGet. Prueba otra red o el punto
de acceso del telefono, desactiva temporalmente una VPN/proxy y ejecuta:

```powershell
ipconfig /flushdns
nslookup api.nuget.org
Test-NetConnection api.nuget.org -Port 443
```

No modifiques los archivos `.cs` para corregir este problema. Los `using`
necesarios ya estan incluidos; primero debe completarse la restauracion.
