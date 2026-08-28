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

/* Construye una mascara con un solo bit en la posicion indicada. */
#define BIT_GPIO(posicion)          (1UL << (posicion))

/* Convierte un numero de pin de 0 a 15 en una mascara de 16 bits. */
#define MASCARA_PIN_GPIO(pin)       ((uint16_t)BIT_GPIO(pin))

/* Mascara correspondiente a los 16 bits de un puerto GPIO. */
#define MASCARA_PUERTO_GPIO         0xFFFFUL

/* Cada pin utiliza un campo de dos bits en MODER, OSPEEDR y PUPDR. */
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

/* Bits de RCC_IOPENR que habilitan el reloj de cada puerto. */
#define RELOJ_PUERTO_A              BIT_GPIO(0U)
#define RELOJ_PUERTO_B              BIT_GPIO(1U)
#define RELOJ_PUERTO_C              BIT_GPIO(2U)
#define RELOJ_PUERTO_D              BIT_GPIO(3U)
#define RELOJ_PUERTO_F              BIT_GPIO(5U)

/* ------------------------------------------------------------
 * ESTRUCTURAS PARA MAPEAR LOS REGISTROS
 * ------------------------------------------------------------ */

/*
 * Bloque reducido del RCC.
 * Los 13 elementos reservados ocupan 13 x 4 = 52 bytes = 0x34.
 * Por eso IOPENR queda exactamente en el offset 0x34.
 */
typedef struct
{
    volatile uint32_t RESERVADO_00_A_30[13];   /*Arreglo de 13 elemento*/
    volatile uint32_t IOPENR;                  /* Offset 0x34. */
} BloqueRelojGpio;

/*
 * Bloque de registros de un puerto GPIO.
 * El orden de los campos coincide con los offsets del periférico.
 */
typedef struct
{
    volatile uint32_t MODER;             /* 0x00: modo del pin. */
    volatile uint32_t OTYPER;            /* 0x04: tipo de salida. */
    volatile uint32_t OSPEEDR;           /* 0x08: velocidad. */
    volatile uint32_t PUPDR;             /* 0x0C: pull-up/pull-down. */
    volatile uint32_t IDR;               /* 0x10: datos de entrada. */
    volatile uint32_t ODR;               /* 0x14: datos de salida. */
    volatile uint32_t BSRR;              /* 0x18: set/reset de bits. */
    volatile uint32_t LCKR;              /* 0x1C: bloqueo. */
    volatile uint32_t AFR[2];            /* 0x20 y 0x24: funciones alternativas. */
    volatile uint32_t BRR;               /* 0x28: reset de bits. */
} BloquePuertoGpio;

/* Punteros simbólicos a los bloques mapeados en memoria. */
#define BLOQUE_RCC_GPIO             ((BloqueRelojGpio *)DIRECCION_BASE_RCC)
#define BLOQUE_GPIO_A               ((BloquePuertoGpio *)DIRECCION_BASE_GPIOA)
#define BLOQUE_GPIO_B               ((BloquePuertoGpio *)DIRECCION_BASE_GPIOB)
#define BLOQUE_GPIO_C               ((BloquePuertoGpio *)DIRECCION_BASE_GPIOC)
#define BLOQUE_GPIO_D               ((BloquePuertoGpio *)DIRECCION_BASE_GPIOD)
#define BLOQUE_GPIO_F               ((BloquePuertoGpio *)DIRECCION_BASE_GPIOF)

/* ------------------------------------------------------------
 * CONSTANTES DE PIN
 * ------------------------------------------------------------ */

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

/* ------------------------------------------------------------
 * ENUMERACIONES Y TIPOS PUBLICOS
 * ------------------------------------------------------------ */

/* Puertos digitales soportados por la libreria. */
typedef enum
{
    PUERTO_A = 0,
    PUERTO_B,
    PUERTO_C,
    PUERTO_D,
    PUERTO_F
} PuertoDigital;

/* Niveles logicos utilizados para leer y escribir pines. */
typedef enum
{
    NIVEL_BAJO = 0,
    NIVEL_ALTO = 1
} NivelDigital;

/* Resultado devuelto por las operaciones de la libreria. */
typedef enum
{
    GPIO_EXITO = 0,
    GPIO_ERROR_PUERTO,
    GPIO_ERROR_PIN,
    GPIO_ERROR_MASCARA,
    GPIO_ERROR_VALOR,
    GPIO_ERROR_PUNTERO
} EstadoGpio;

/* ------------------------------------------------------------
 * PROTOTIPOS DE LAS FUNCIONES PUBLICAS
 * ------------------------------------------------------------ */

/* Habilita el reloj del puerto GPIO seleccionado. */
EstadoGpio puerto_habilitar(PuertoDigital puerto);

/* Configura un pin como entrada digital sin resistencia interna. */
EstadoGpio pin_configurar_entrada(PuertoDigital puerto, uint8_t pin);

/* Configura un pin como salida push-pull y velocidad baja. */
EstadoGpio pin_configurar_salida(PuertoDigital puerto, uint8_t pin);

/* Lee un pin individual y entrega el nivel mediante un puntero. */
EstadoGpio pin_leer(PuertoDigital puerto, uint8_t pin, NivelDigital *nivel);

/* Escribe NIVEL_BAJO o NIVEL_ALTO en un pin individual. */
EstadoGpio pin_escribir(PuertoDigital puerto, uint8_t pin, NivelDigital nivel);

/* Invierte el estado almacenado de un pin configurado como salida. */
EstadoGpio pin_alternar(PuertoDigital puerto, uint8_t pin);

/* Lee los bits seleccionados de IDR usando una mascara. */
EstadoGpio puerto_leer_bits(PuertoDigital puerto, uint16_t mascara, uint16_t *valor);

/* Escribe un patron en los bits seleccionados sin afectar los demas. */
EstadoGpio puerto_escribir_bits(PuertoDigital puerto, uint16_t mascara, uint16_t valor);

#endif /* GPIO_H_ */
