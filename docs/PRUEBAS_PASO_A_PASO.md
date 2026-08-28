# Pruebas paso a paso

Realiza las pruebas en este orden. No conectes motor y expulsor hasta que entradas, LEDs y UART estén verificados.

## 1. Compilación

1. Importa `SICAVI_STM32_G071RB` en STM32CubeIDE.
2. Limpia y compila.
3. El resultado debe ser cero errores.
4. Abre `CSharp/SICAVI_Camara_STM32/SICAVI.sln`.
5. Restaura NuGet y recompila en `Debug | x64`.

## 2. Solo la placa y los LEDs

1. Conecta el LED rojo en PA6 y el amarillo en PA7, ambos con 330 Ω.
2. Carga el firmware.
3. En estado detenido deben quedar apagados los tres indicadores.
4. Conecta la LDR antes de probar RUN: la configuración entregada habilita el bloqueo por iluminación entre 25 % y 90 %.

## 3. LDR

1. Conecta el divisor LDR indicado en el mapa de conexiones.
2. Abre un terminal serial a 115200 o conecta C#.
3. Observa `LIGHT`, `ADC` y `MV` en `TEL=STATUS`.
4. Cubre la LDR y luego ilumínala.
5. Verifica que los valores cambien claramente y permanezcan entre 10 % y 95 % para la prueba normal.
6. Si tu montaje trabaja en un rango diferente, ajusta los límites en `Inc/sicavi_config.h`.

## 4. UART y C#

1. Cierra cualquier terminal que esté usando el puerto COM.
2. Asigna el puerto virtual del ST-LINK a `COM6` desde el Administrador de dispositivos de Windows.
3. Ejecuta C#; la conexión se realiza automáticamente y se reintenta cada dos segundos si COM6 no está disponible.
4. Deben actualizarse estado, LDR, horómetro de trabajo, ciclos y alarma sin pulsar ningún botón de conexión.
5. Confirma que la cámara permanezca cerrada hasta pulsar **RUN**.

## 5. RUN y STOP sin relé

1. Pulsa RUN desde C#.
2. Debe encenderse el LED amarillo de estado; verde y rojo conservan la indicación de la última clasificación.
3. Pulsa el STOP físico de PC1.
4. Deben apagarse los tres indicadores al quedar detenido.
5. Repite usando B1 para START y el botón STOP de C#.

## 6. Simular el sensor de caja

1. Inicia la cámara y selecciona el color y la forma esperados.
2. Pulsa RUN.
3. Con `D8 / PA9` normalmente en alto, llévalo brevemente a GND para simular la detección; nunca apliques 5 V.
4. Confirma que el motor continúa 400 ms después del flanco y luego se apaga con la caja dentro de la ROI. Ajusta este valor sólo si cambia la distancia mecánica entre D8 y la cámara.
5. Tras 300 ms de estabilización, C# debe registrar una caja con ID, reservar un fotograma nuevo y responder automáticamente.
6. Confirma que la imagen y la máscara permanecen fijas hasta activar nuevamente el sensor con otra caja.
7. Comprueba `EVT=FINISHED` si la decisión es GOOD o REJECT.

## 7. Relé sin motor

1. Monta el transistor, la resistencia de 1 kΩ y el diodo.
2. Alimenta solamente la bobina del relé; deja desconectados COM/NO/NC.
3. Pulsa RUN y escucha/observa la activación del relé del motor.
4. Simula una caja y verifica que el relé se desactive mientras C# analiza.
5. Prueba un rechazo y verifica el relé del expulsor durante aproximadamente 500 ms.

## 8. Motor y maqueta

1. Conecta el motor mediante los contactos del relé y una fuente adecuada.
2. Mantén una parada física accesible.
3. Prueba primero sin caja y sin carga mecánica.
4. Comprueba el sentido y que STOP quite energía de control.
5. Agrega una caja y confirma la secuencia completa.

## 9. Pruebas de falla

| Prueba | Resultado esperado |
|---|---|
| Desconectar C# mientras está en RUN | Alarma `COMM_TIMEOUT`, motor apagado. |
| Caja ya presente al pulsar RUN | El STM32 rechaza RUN con `BOX_SENSOR_ACTIVE`; no se toma una captura. |
| No responder a una caja | Alarma `VISION_TIMEOUT` a los ocho segundos. |
| LDR fuera del rango configurado | RUN se rechaza con `LIGHT_OUT_OF_RANGE`, porque `SICAVI_BLOQUEO_POR_LUZ_HABILITADO` está en `1U`. |
| Enviar un ID incorrecto | Respuesta `ID_MISMATCH`; no acciona expulsor. |
| Pulsar STOP durante cualquier operación | Motor y expulsor apagados. |

## Registro recomendado

Para cada prueba anota fecha, conexión, valor ADC, resultado, tiempo del motor, ciclos del expulsor y cualquier modificación. No presentes como verificada una prueba física que todavía no ejecutaste.
