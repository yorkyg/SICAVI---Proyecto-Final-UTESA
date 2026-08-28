# Biblioteca GPIO de referencia

Esta carpeta conserva la estructura académica original de `gpio.h` y `gpio.c` usada como contrato para la implementación del firmware.

La versión integrada y operativa está en:

- `SICAVI_STM32_G071RB/Inc/gpio.h`
- `SICAVI_STM32_G071RB/Src/gpio.c`

La implementación final mantiene macros, estructuras de registros, enumeraciones, validación de argumentos y funciones públicas de la biblioteca base. El resto del firmware consume esa interfaz para GPIO digital, mientras ADC, EXTI, temporizador y UART usan los recursos específicos del STM32G071RB.

