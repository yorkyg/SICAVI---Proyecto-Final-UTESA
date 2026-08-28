# Plan completo de desarrollo de SICAVI

## Alcance final

SICAVI inspeccionará cajas por **color + forma**, coordinará el proceso físico mediante un STM32G071RB y almacenará la trazabilidad en SQL Server.

## Arquitectura objetivo

```text
Webcam USB -> Windows Forms + OpenCvSharp -> decisión GOOD/REJECT/UNKNOWN
                         |
                         +-> SQL Server: productos, lotes e inspecciones
                         |
                         +<-> UART/USB <-> STM32G071RB
                                          |- Sensor de presencia
                                          |- LDR por ADC
                                          |- Motor ON/OFF
                                          |- Expulsor ON/OFF
                                          |- LEDs RUN/STOP/ALARMA
                                          |- Pulsadores
                                          |- Timer y horómetro
```

## Fases

| Semana | Trabajo | Resultado verificable |
|---:|---|---|
| 1 | Solución Windows Forms, webcam, captura, ROI y registro de eventos | Video estable y capturas guardadas |
| 2 | Clasificación HSV y preparación de muestras | Verde, rojo o desconocido |
| 3 | Contornos, vértices y circularidad | Círculo, cuadrado, triángulo o desconocido |
| 4 | Decisión compuesta, calibración y pruebas repetibles | Color + forma con motivo de rechazo |
| 5 | Modelo de datos, SQL Server, ADO.NET y CRUD de productos | Productos y parámetros persistentes |
| 6 | GPIO, ADC, LEDs, Timer, motor, expulsor y máquina de estados | Firmware probado por módulos |
| 7 | UART, Regex, ID de caja, timeout y confirmaciones | Comunicación C#-STM32 estable |
| 8 | Integración, pruebas de seguridad, historial y documentación | Ciclo completo demostrable |

## Entidades de base de datos

### Productos

- Id.
- Código.
- Descripción.
- Color esperado.
- Forma esperada.
- Rangos HSV.
- Porcentaje mínimo de color.
- Área mínima y circularidad.
- Activo.

### Lotes

- Id.
- ProductoId.
- Fecha de inicio y cierre.
- Estado.
- Contadores.

### Inspecciones

- Id único de caja.
- LoteId.
- Color y forma detectados.
- Porcentajes HSV.
- Vértices y circularidad.
- Resultado y motivo.
- Fecha y ruta de imagen.

### LecturasHorometro

- Segundos del sistema.
- Segundos del motor.
- Ciclos del expulsor.
- Fecha de lectura.

## Máquina de estados del STM32

1. `DETENIDO`.
2. `VERIFICANDO_LUZ`.
3. `TRANSPORTANDO`.
4. `ESPERANDO_RESULTADO`.
5. `DEJANDO_PASAR`.
6. `RECHAZANDO`.
7. `ALARMA`.

Reglas de seguridad:

- Motor y expulsor apagados durante el arranque.
- `STOP` con prioridad.
- Detención ante pérdida de UART.
- Timeout esperando el resultado de C#.
- Alarma si el sensor queda bloqueado.
- Alarma o inhibición si la iluminación sale de rango.
- No aceptar una respuesta con ID diferente al de la caja actual.

## Tramas UART previstas

```text
EVT=DETECTED;ID=0001;LIGHT=72
CMD=GOOD;ID=0001
CMD=REJECT;ID=0001
CMD=UNKNOWN;ID=0001
EVT=FINISHED;ID=0001;RESULT=REJECT
TEL=STATUS;HOURMETER_S=2845;CYCLES=128
```

La clase `SerialMessageParser` valida la estructura por Regex. La integración actual agrega `SerialConnectionService` y `SicaviSerialController`, con eventos conectados al formulario principal.

## Criterio de finalización

El proyecto estará completo cuando una caja sea detectada físicamente, inspeccionada por color y forma, procesada por el STM32, confirmada mediante UART y registrada una sola vez en SQL Server junto con lote, resultado y horómetro.
