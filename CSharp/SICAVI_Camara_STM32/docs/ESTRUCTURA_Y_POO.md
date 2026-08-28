# Estructura y conceptos de Programación II

| Concepto | Aplicación en el proyecto |
|---|---|
| Encapsulamiento | Servicios de cámara y analizadores contienen su propia lógica y estado. |
| Herencia | `ColorAnalyzer` y `ShapeAnalyzer` heredan de `VisionAnalyzerBase<TResult>`. |
| Polimorfismo | Cada analizador implementa `Analyze(Mat image)` con comportamiento propio. |
| Excepciones | `CameraException` representa fallos controlados de inicialización. |
| Delegados y eventos | Cámara, telemetría, caja detectada, fin de inspección y alarmas. |
| Regex | `SerialMessageParser` valida las tramas UART actuales. |
| StringBuilder | `AppLogger` construye el historial sin concatenación repetitiva. |
| Colecciones | Contornos, candidatos de formas y campos de tramas. |
| Enumeraciones | Colores, formas, objetivos y decisiones. |
| ADO.NET | Repositorios SQL con `Microsoft.Data.SqlClient`, consultas parametrizadas y transacciones para el horómetro. |
| Seguridad | Contraseñas almacenadas con PBKDF2-SHA256, sal aleatoria y comparación en tiempo constante. |

## Responsabilidad de cada carpeta

- `Forms`: interfaz y eventos de controles.
- `Services`: captura y recursos externos.
- `Vision`: algoritmos HSV, contornos y decisión compuesta.
- `Models`: resultados y enumeraciones.
- `Configuration`: parámetros ajustables.
- `Communication`: puerto COM, protocolo UART, comandos, telemetría y eventos STM32.
- `Data`: inicialización de SQL Server y repositorios de usuarios y producción.
- `Database`: script idempotente que crea la base y las tablas.
- `Utilities`: utilidades compartidas como el registro de eventos.
- `Exceptions`: errores específicos del dominio.
