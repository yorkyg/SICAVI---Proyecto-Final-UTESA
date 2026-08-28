# Protocolo de primeras pruebas de cámara

## Datos de la prueba

| Campo | Valor |
|---|---|
| Fecha |  |
| Responsable |  |
| Webcam |  |
| Resolución |  |
| Distancia cámara-caja |  |
| Iluminación |  |
| Versión del proyecto | 1.0 inicial |

## Pruebas funcionales

| ID | Prueba | Procedimiento | Resultado esperado | Cumple |
|---|---|---|---|---|
| CAM-01 | Abrir cámara automática | Presionar RUN | Cámara 0 a 1920x1080, video visible y FPS mayor que cero |  |
| CAM-02 | Detener cámara automática | Presionar STOP | Video deja de actualizarse sin bloquear la ventana |  |
| CAM-03 | Reconectar | Presionar RUN nuevamente | La cámara vuelve a mostrar video |  |
| CAM-04 | Retención de inspección | Detectar y clasificar una caja | Imagen anotada y máscara permanecen fijas hasta la caja siguiente, aun con alarma o STOP |  |
| CAM-04 | Cámara ocupada | Abrirla previamente en otra aplicación y pulsar RUN | Mensaje controlado y RUN cancelado |  |
| VIS-01 | Caja verde | Colocar verde dentro de ROI | Verde por encima del umbral |  |
| VIS-02 | Caja roja | Colocar roja dentro de ROI | Rojo por encima del umbral |  |
| VIS-03 | Círculo | Etiqueta blanca con círculo negro | Forma círculo |  |
| VIS-04 | Cuadrado | Etiqueta blanca con cuadrado negro | Forma cuadrado |  |
| VIS-05 | Triángulo | Etiqueta blanca con triángulo negro | Forma triángulo |  |
| VIS-06 | Coincidencia | Seleccionar color y forma correctos | APROBADA |  |
| VIS-07 | Color incorrecto | Seleccionar otro color | RECHAZADA por color |  |
| VIS-08 | Forma incorrecta | Seleccionar otra forma | RECHAZADA por forma |  |
| VIS-09 | Imagen deficiente | Ocultar parcialmente la figura | INDETERMINADA |  |

## Registro de ajustes

| Parámetro | Valor anterior | Valor nuevo | Motivo | Evidencia |
|---|---:|---:|---|---|
| `minimumColorPercentage` | 8.0 |  |  |  |
| `minimumShapeAreaPixels` | 900 |  |  |  |
| `polygonEpsilonRatio` | 0.025 |  |  |  |
| `minimumCircleCircularity` | 0.74 |  |  |  |

## Regla de prueba

Cambia un parámetro por vez, repite la misma muestra al menos cinco veces y conserva una captura antes y después. No se reportará precisión hasta disponer de una cantidad suficiente de pruebas controladas.
