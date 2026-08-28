# SICAVI - Cámara, visión y control STM32

Solución de Visual Studio para el proyecto **SICAVI - Sistema de Inspección y Clasificación Automática por Visión**.

Permite probar una webcam USB, analizar color por HSV, reconocer una figura geométrica, controlar la NUCLEO-G071RB mediante UART y conservar la operación en SQL Server. La aplicación está hecha en **C#**, **Windows Forms**, **.NET 8**, **OpenCvSharp4** y **Microsoft.Data.SqlClient**.

## Funciones incluidas

- Usar siempre la cámara 0 y abrirla automáticamente a `1920x1080` al activar INICIAR.
- Mantener la cámara activa durante la sesión de producción, incluso ante una alarma, y cerrarla al pulsar DETENER, cerrar sesión o salir.
- Mostrar video en vivo sin bloquear la interfaz.
- Dibujar una región de interés central - ROI.
- Detectar color verde, rojo o desconocido mediante HSV.
- Reconocer círculo, cuadrado, triángulo o forma desconocida mediante contornos.
- Comparar el color y la forma detectados con los esperados.
- Emitir `APROBADA`, `RECHAZADA` o `INDETERMINADA`.
- Mostrar máscara HSV, contorno, vértices y circularidad.
- Ignorar el borde blanco de la caja al reconocer la forma, usando el objeto saturado central.
- Mantener fija la última inspección hasta que el sensor anuncie la caja siguiente.
- Solicitar usuario y clave antes de mostrar el panel principal.
- Registrar operadores y administrar cuentas, roles y estados desde un formulario exclusivo.
- Guardar clasificaciones, conteos por fecha, horómetro diario/total y alarmas en SQL Server.
- Consultar inspecciones, horómetro y alarmas desde **Datos de producción**.
- Conectar automáticamente COM6 a 115200 bit/s, con reintento cada dos segundos.
- Enviar RUN, STOP y RESET.
- Mostrar LDR, estado, un único horómetro de trabajo, ciclos y alarmas recibidas.
- Responder automáticamente GOOD, REJECT o UNKNOWN a una caja detectada.
- Mantener un latido de comunicación cada segundo.

## Base de datos y acceso inicial

Al iniciar, SICAVI prepara automáticamente la base `SICAVI` y sus tablas en la instancia configurada en `src/Sicavi.WinForms/database-settings.json`. La configuración incluida usa `.\WINCC` con autenticación de Windows.

Primer acceso:

```text
Usuario: admin
Clave:   Sicavi@123
```

Cambia esta clave desde **GESTIÓN DE USUARIOS** después de entrar. Consulta [docs/BASE_DE_DATOS_Y_USUARIOS.md](docs/BASE_DE_DATOS_Y_USUARIOS.md).

## Requisitos de la computadora

- Windows 10 u 11 de 64 bits.
- Visual Studio 2022 actualizado.
- Carga de trabajo **Desarrollo de escritorio de .NET**.
- SDK de .NET 8.
- Webcam USB o integrada.
- Acceso a Internet durante la primera restauración de paquetes NuGet.
- SQL Server en ejecución y permisos de Windows para crear/usar la base `SICAVI`.

## Inicio rápido

1. Descomprime la carpeta completa.
2. Abre `SICAVI.sln` en Visual Studio 2022.
3. Espera a que Visual Studio termine de restaurar los paquetes.
4. Verifica que la configuración superior sea `Debug` y `x64`.
5. Presiona `F5` o el botón verde **SICAVI**.
6. Inicia sesión con el administrador inicial o registra un operador.
7. Coloca una caja verde o roja dentro del rectángulo de la ROI.
8. Coloca sobre ella una etiqueta blanca con una figura roja o verde.
9. Confirma que la NUCLEO utiliza COM6 y que la webcam deseada corresponde al índice 0 de Windows/OpenCV.
10. Pulsa **INICIAR**: SICAVI confirma REINICIAR, abre la cámara 0 a 1920x1080 y confirma el arranque del STM32.
11. Pulsa **DETENER** para detener la maqueta y cerrar la cámara.

La documentación completa de la entrega está en el [README principal](../../README.md), la [guía de instalación y uso](../../docs/GUIA_INSTALACION_Y_USO.md) y el [informe final C#](../../docs/INFORME_FINAL_CSHARP.md).

Lee la [GUIA_DE_USO.md](GUIA_DE_USO.md) antes de comenzar las pruebas.

## Estructura principal

```text
SICAVI_Camara_STM32
├── SICAVI.sln
├── README.md
├── GUIA_DE_USO.md
├── docs
├── assets
└── src
    └── Sicavi.WinForms
        ├── Communication
        ├── Configuration
        ├── Data
        ├── Database
        ├── Exceptions
        ├── Forms
        ├── Models
        ├── Services
        ├── Utilities
        └── Vision
```

## Paquetes utilizados

- `OpenCvSharp4.Windows` `4.13.0.20260627`
- `OpenCvSharp4.Extensions` `4.13.0.20260627`
- `System.IO.Ports` `8.0.0`
- `Microsoft.Data.SqlClient` `7.0.2`

OpenCvSharp es el wrapper de OpenCV para .NET. Referencias: [repositorio oficial](https://github.com/shimat/opencvsharp) y [paquete NuGet](https://www.nuget.org/packages/OpenCvSharp4/).
