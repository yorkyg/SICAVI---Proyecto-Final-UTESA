# Mapa de pines y conexiones

## Asignación de SICAVI

| Función | Pin STM32 | Conector conocido | Tipo | Nivel activo |
|---|---|---|---|---|
| LDR | `PA0` | STM `A0` | ADC 12 bits | Analógico 0–3.3 V |
| START local | `PC13` | Pulsador B1 de la placa | Entrada | Bajo |
| STOP externo | `PC1` | ST Morpho | Entrada | Bajo |
| Sensor de caja | `PA9` | STTM `D8` | Entrada | Bajo |
| Control del motor | `PC7` | STM `D9` | Salida | Alto |
| Relé del expulsor | `PB1` | STM `A3` | Salida | Alto |
| LED verde ACEPTADA | `PA5` | `D13`, LED LD4 integrado | Salida | Alto |
| LED rojo RECHAZADA | `PA6` | STM `D12` | Salida | Alto |
| LED amarillo ESTADO/ALARMA | `PA7` | STM `D11` | Salida | Alto/parpadeo |
| UART TX | `PA2` | ST-LINK VCP | USART2 | 115200 8-N-1 |
| UART RX | `PA3` | ST-LINK VCP | USART2 | 115200 8-N-1 |

El sensor de caja se conecta directamente a `D8` de la hilera STM digital (`PA9`). Esta entrada usa pull-up e interrupción EXTI por flanco descendente para apagar el motor sin esperar el antirrebote de los botones. `PC1`, usado por STOP, continúa en el encabezado largo **ST Morpho**.

## LDR

```text
3V3 de la NUCLEO
       |
      LDR
       |
       +-------------- PA0 / A0
       |
     10 kΩ
       |
      GND
```

Con esta polarización, normalmente más luz produce mayor voltaje y un porcentaje ADC mayor. `PA0` nunca debe superar 3.3 V.

## Botón STOP externo

```text
3V3 ---- 10 kΩ ----+---- PC1
                   |
                 pulsador
                   |
                  GND
```

Sin pulsar, `PC1` lee alto. Al pulsarlo, lee bajo y el firmware detiene inmediatamente motor y expulsor.

## Sensor de caja

Para un sensor o módulo con salida compatible con 3.3 V:

```text
VCC del sensor  ---- alimentación indicada por su fabricante
GND del sensor  ---- GND NUCLEO
OUT del sensor  ---- D8 / PA9
```

No conectes directamente a `D8 / PA9` una salida de 5 V ni un sensor industrial de 12/24 V. Para esos casos usa optoacoplador, transistor o adaptación de nivel. El firmware espera que `OUT` baje a GND al detectar; sin caja debe permanecer alto.

## Indicadores LED

```text
PA6 ---- 330 Ω ----|>|---- GND     LED rojo RECHAZADA
PA7 ---- 330 Ω ----|>|---- GND     LED amarillo ESTADO/ALARMA
```

La pata larga del LED es el ánodo y mira hacia el pin mediante la resistencia. `PA5` utiliza el LED verde LD4 ya integrado en la NUCLEO. Con el sistema detenido los tres indicadores quedan apagados; durante RUN el amarillo permanece encendido, y en alarma parpadea. Verde o rojo muestran la última decisión válida.

## Etapa de relé del motor

Repite la misma etapa con `PB1` para el expulsor.

```text
                                 +5 V externo
                                     |
                                bobina del relé
                                     |
                                     +------|<|------+ 
                                     |    1N4007     |
                                     | ánodo  cátodo |
                                     C              +5 V
PC7 / D9 ---- 1 kΩ ---- B        2N2222
                                     E
                                     |
GND NUCLEO --------------------------+---- GND fuente 5 V
```

Orientación del diodo: **cátodo al +5 V** y **ánodo al colector**. El contacto `COM/NO/NC` del relé controla la alimentación del motor y está eléctricamente separado de la bobina.

Si usas un módulo de relé comercial con transistor y diodo incorporados, conecta `VCC`, `GND` e `IN`, pero comprueba que `IN` reconozca 3.3 V. Algunos módulos son activos en bajo; en ese caso cambia `SICAVI_DRIVER_MOTOR_ACTIVO_ALTO` o `SICAVI_RELE_EXPULSOR_ACTIVO_ALTO` a `0U` dentro de `Inc/sicavi_config.h`.

## Alimentación

- La NUCLEO puede alimentar la lógica de prueba desde el USB del ST-LINK.
- La bobina de 5 V y el motor deben usar una fuente apropiada para su corriente.
- Une las tierras de la NUCLEO y la fuente de 5 V cuando uses transistor sin aislamiento.
- No alimentes el motor desde el pin 5 V de la NUCLEO.
