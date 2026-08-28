#ifndef GPIO_H_
#define GPIO_H_

#include <stdint.h>

/* ============================================================
 * LIBRERIA GPIO DIGITAL - STM32G071RB
 *
 * Archivo de cabecera (.h).
 * Contiene las macros, direcciones, estructuras de registros,
 * enumeraciones, tipos de datos, constantes y prototipos.
 *
 * Las funciones se implementan en gpio.c. El programa main.c
 * debe usar las funciones publicas y no manipular registros.
 * ============================================================ */

/* ------------------------------------------------------------
 * MACROS GENERALES DE BITS Y CAMPOS
 * ------------------------------------------------------------ */
#define BIT_GPIO(posicion)          (1UL << (posicion))
#define MASCARA_PIN_GPIO(pin)       ((uint16_t)BIT_GPIO(pin))
#define MASCARA_PUERTO_GPIO         0xFFFFUL
#define MASCARA_CAMPO_2_BITS        0x3UL
#define VALOR_MODO_ENTRADA          0x0UL
#define VALOR_MODO_SALIDA           0x1UL
#define VALOR_SIN_RESISTENCIA       0x0UL
#define VALOR_VELOCIDAD_BAJA        0x0UL

/* ------------------------------------------------------------
 * DIRECCIONES BASE DEL STM32G071RB
 * ------------------------------------------------------------ */
#define DIRECCION_BASE_RCC          0x40021000UL
#define DIRECCION_BASE_GPIOA        0x50000000UL
#define DIRECCION_BASE_GPIOB        0x50000400UL
#define DIRECCION_BASE_GPIOC        0x50000800UL
#define DIRECCION_BASE_GPIOD        0x50000C00UL
#define DIRECCION_BASE_GPIOF        0x50001400UL

#define RELOJ_PUERTO_A              BIT_GPIO(0U)
#define RELOJ_PUERTO_B              BIT_GPIO(1U)
#define RELOJ_PUERTO_C              BIT_GPIO(2U)
#define RELOJ_PUERTO_D              BIT_GPIO(3U)
#define RELOJ_PUERTO_F              BIT_GPIO(5U)

/* ------------------------------------------------------------
 * ESTRUCTURAS PARA MAPEAR LOS REGISTROS
 * ------------------------------------------------------------ */
/* Los 13 elementos reservados colocan IOPENR en el offset 0x34. */
typedef struct
{
    volatile uint32_t RESERVADO_00_A_30[13];
    volatile uint32_t IOPENR;
} BloqueRelojGpio;

typedef struct
{
    volatile uint32_t MODER;
    volatile uint32_t OTYPER;
    volatile uint32_t OSPEEDR;
    volatile uint32_t PUPDR;
    volatile uint32_t IDR;
    volatile uint32_t ODR;
    volatile uint32_t BSRR;
    volatile uint32_t LCKR;
    volatile uint32_t AFR[2];
    volatile uint32_t BRR;
} BloquePuertoGpio;

#define BLOQUE_RCC_GPIO             ((BloqueRelojGpio *)DIRECCION_BASE_RCC)
#define BLOQUE_GPIO_A               ((BloquePuertoGpio *)DIRECCION_BASE_GPIOA)
#define BLOQUE_GPIO_B               ((BloquePuertoGpio *)DIRECCION_BASE_GPIOB)
#define BLOQUE_GPIO_C               ((BloquePuertoGpio *)DIRECCION_BASE_GPIOC)
#define BLOQUE_GPIO_D               ((BloquePuertoGpio *)DIRECCION_BASE_GPIOD)
#define BLOQUE_GPIO_F               ((BloquePuertoGpio *)DIRECCION_BASE_GPIOF)

/* ------------------------------------------------------------
 * CONSTANTES DE PIN
 * ------------------------------------------------------------
 * La libreria modelo usa indices 0..15. SICAVI omite solamente estos
 * nombres cuando el HAL ya los definio como mascaras; el contrato de la
 * libreria y su implementacion no cambian.
 */
#ifndef GPIO_OMITIR_CONSTANTES_PIN
#define GPIO_PIN_0                  0U
#define GPIO_PIN_1                  1U
#define GPIO_PIN_2                  2U
#define GPIO_PIN_3                  3U
#define GPIO_PIN_4                  4U
#define GPIO_PIN_5                  5U
#define GPIO_PIN_6                  6U
#define GPIO_PIN_7                  7U
#define GPIO_PIN_8                  8U
#define GPIO_PIN_9                  9U
#define GPIO_PIN_10                 10U
#define GPIO_PIN_11                 11U
#define GPIO_PIN_12                 12U
#define GPIO_PIN_13                 13U
#define GPIO_PIN_14                 14U
#define GPIO_PIN_15                 15U
#endif

typedef enum
{
    PUERTO_A = 0,
    PUERTO_B,
    PUERTO_C,
    PUERTO_D,
    PUERTO_F
} PuertoDigital;

typedef enum
{
    NIVEL_BAJO = 0,
    NIVEL_ALTO = 1
} NivelDigital;

typedef enum
{
    GPIO_EXITO = 0,
    GPIO_ERROR_PUERTO,
    GPIO_ERROR_PIN,
    GPIO_ERROR_MASCARA,
    GPIO_ERROR_VALOR,
    GPIO_ERROR_PUNTERO
} EstadoGpio;

EstadoGpio puerto_habilitar(PuertoDigital puerto);
EstadoGpio pin_configurar_entrada(PuertoDigital puerto, uint8_t pin);
EstadoGpio pin_configurar_salida(PuertoDigital puerto, uint8_t pin);
EstadoGpio pin_leer(PuertoDigital puerto, uint8_t pin, NivelDigital *nivel);
EstadoGpio pin_escribir(PuertoDigital puerto, uint8_t pin, NivelDigital nivel);
EstadoGpio pin_alternar(PuertoDigital puerto, uint8_t pin);
EstadoGpio puerto_leer_bits(PuertoDigital puerto, uint16_t mascara, uint16_t *valor);
EstadoGpio puerto_escribir_bits(PuertoDigital puerto, uint16_t mascara, uint16_t valor);

#endif /* GPIO_H_ */
