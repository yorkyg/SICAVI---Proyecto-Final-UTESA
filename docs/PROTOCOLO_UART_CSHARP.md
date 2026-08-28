# Protocolo UART y manejo desde C#

## Configuración

```text
USART2: PA2 TX / PA3 RX
Velocidad: 115200 bit/s
Formato: 8 bits, sin paridad, 1 bit de parada
Terminación: \r\n
Transporte: puerto COM virtual del ST-LINK
```

Los mensajes son ASCII. El primer campo indica la categoría y los demás campos se separan con punto y coma.

## Comandos C# → STM32

| Comando | Acción |
|---|---|
| `CMD=RUN` | Arranca el transporte si no hay alarma, el sensor de caja está libre y la iluminación es válida. |
| `CMD=STOP` | Apaga motor y expulsor. |
| `CMD=RESET` | Borra la alarma y vuelve a STOP; no borra el horómetro. |
| `CMD=STATUS` | Solicita telemetría inmediata. |
| `CMD=PING` | Latido automático; se envía cada segundo. |
| `CMD=GOOD;ID=1` | Aprueba la caja identificada. |
| `CMD=REJECT;ID=1` | Rechaza la caja identificada. |
| `CMD=UNKNOWN;ID=1` | Informa que visión no pudo decidir. |

## Eventos STM32 → C#

| Trama | Significado |
|---|---|
| `EVT=BOOT;FW=1.3.0;BOARD=NUCLEO_G071RB` | Firmware iniciado. |
| `EVT=RUNNING;SOURCE=C_SHARP` | Sistema en marcha. |
| `EVT=STOPPED;SOURCE=LOCAL` | Parada local o remota. |
| `EVT=DETECTED;ID=1;LIGHT=72;ADC=2948` | Caja posicionada y estable; C# debe capturar y analizar. |
| `EVT=FINISHED;ID=1;RESULT=GOOD` | Terminó el ciclo físico. |
| `EVT=ALARM;CODE=VISION_TIMEOUT` | Alarma activa. |

La telemetría se emite cada segundo:

```text
TEL=STATUS;FW=1.3.0;STATE=TRANSPORTING;MOTOR=1;BOX_SENSOR=0;LIGHT=72;ADC=2948;MV=2375;HOURMETER_S=2845;CYCLES=128;ALARM=NONE
```

## Archivos de C#

| Archivo | Responsabilidad |
|---|---|
| `Communication/SerialConnectionService.cs` | Abre/cierra COM y arma líneas completas. |
| `Communication/SerialMessageParser.cs` | Valida y separa tramas con Regex. |
| `Communication/SicaviSerialController.cs` | Expone comandos, telemetría y eventos de alto nivel. |
| `Communication/SicaviTelemetry.cs` | Modelos de datos recibidos. |
| `Forms/MainForm.cs` | Conecta controles, cámara, visión y STM32. |

## Uso independiente desde C#

```csharp
using Sicavi.WinForms.Communication;

var stm32 = new SicaviSerialController();

stm32.TelemetryReceived += (_, telemetry) =>
{
    Console.WriteLine($"Estado: {telemetry.State}");
    Console.WriteLine($"LDR: {telemetry.LightPercentage}%");
    Console.WriteLine($"Horómetro: {telemetry.WorkTime}");
};

stm32.BoxDetected += (_, detection) =>
{
    // Aquí se captura la cámara y se ejecuta OpenCvSharp.
    string resultadoVision = "GOOD";
    stm32.SendVisionResult(detection.Id, resultadoVision);
};

stm32.AlarmRaised += (_, alarm) =>
    Console.WriteLine($"ALARMA: {alarm.Code}");

stm32.Connect("COM6");
stm32.Run();
```

En la aplicación entregada esto ya está conectado. Al recibir `EVT=DETECTED`, `MainForm` reserva un fotograma nuevo y lo analiza; luego convierte `ACEPTADA`, `RECHAZADA` o `INDETERMINADA` en `GOOD`, `REJECT` o `UNKNOWN`. Si la primera lectura es indeterminada se verifican dos fotogramas frescos y sólo se sustituye cuando ambos coinciden.

## Estados y alarmas

Estados posibles:

```text
STOPPED
TRANSPORTING
POSITIONING
STABILIZING
WAITING_RESULT
PASSING
REJECTING
ALARM
```

Alarmas implementadas:

- `LIGHT_OUT_OF_RANGE`: opcional; se habilita con `SICAVI_BLOQUEO_POR_LUZ_HABILITADO=1U` y usa los límites de `sicavi_config.h`.
- `ADC_ERROR`: no terminó una conversión ADC.
- `VISION_TIMEOUT`: C# no respondió en ocho segundos.
- `VISION_UNKNOWN`: OpenCvSharp no pudo decidir.
- `UART_ERROR`: error detectado por USART2.
- `COMM_TIMEOUT`: no llegó el latido de C# durante 3.5 segundos.

Después de una alarma corrige primero la causa y pulsa **REINICIAR**. C# reintenta la orden y sólo cierra la alarma almacenada cuando recibe `ACK=RESET;STATE=STOPPED;ALARM=NONE`. Después puede enviarse RUN.

El receptor del firmware usa una cola de ocho líneas y un vigilante de recuperación de 1800 ms para evitar que una recepción UART incompleta bloquee comandos posteriores.
