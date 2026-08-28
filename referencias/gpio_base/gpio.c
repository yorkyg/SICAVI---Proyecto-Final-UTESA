#include "gpio.h"

/* ============================================================
 * IMPLEMENTACION DE LA LIBRERIA GPIO DIGITAL
 *
 * Este archivo contiene los cuerpos de las funciones y realiza
 * el acceso real a RCC y GPIO mediante las macros y estructuras
 * declaradas en gpio.h.
 * ============================================================ */

/* Obtiene el bloque de registros correspondiente al puerto. */
static BloquePuertoGpio *obtener_bloque_puerto(PuertoDigital puerto)
{
    switch (puerto)
    {
        case PUERTO_A: return BLOQUE_GPIO_A;
        case PUERTO_B: return BLOQUE_GPIO_B;
        case PUERTO_C: return BLOQUE_GPIO_C;
        case PUERTO_D: return BLOQUE_GPIO_D;
        case PUERTO_F: return BLOQUE_GPIO_F;
        default:       return (BloquePuertoGpio *)0;
    }
}

/* Verifica que el numero se encuentre entre 0 y 15. */
static uint8_t pin_es_valido(uint8_t pin)
{
    return (pin <= GPIO_PIN_15) ? 1U : 0U;
}

/*
 * Reemplaza un campo de dos bits sin afectar los otros pines.
 * Se utiliza en MODER, OSPEEDR y PUPDR.
 */
static void escribir_campo_2_bits(volatile uint32_t *registro,
                                  uint8_t pin,
                                  uint32_t valor)
{
    uint32_t desplazamiento = (uint32_t)pin * 2UL;
    uint32_t mascara = MASCARA_CAMPO_2_BITS << desplazamiento;

    *registro = (*registro & ~mascara) |
                ((valor << desplazamiento) & mascara);
}

EstadoGpio puerto_habilitar(PuertoDigital puerto)
{
    uint32_t mascara_reloj;

    switch (puerto)
    {
        case PUERTO_A: mascara_reloj = RELOJ_PUERTO_A; break;
        case PUERTO_B: mascara_reloj = RELOJ_PUERTO_B; break;
        case PUERTO_C: mascara_reloj = RELOJ_PUERTO_C; break;
        case PUERTO_D: mascara_reloj = RELOJ_PUERTO_D; break;
        case PUERTO_F: mascara_reloj = RELOJ_PUERTO_F; break;
        default:       return GPIO_ERROR_PUERTO;
    }

    /* Acceso real al registro RCC_IOPENR. */
    BLOQUE_RCC_GPIO->IOPENR |= mascara_reloj;

    /* Lectura de confirmacion para completar la escritura en el bus. */
    (void)BLOQUE_RCC_GPIO->IOPENR;

    return GPIO_EXITO;
}

EstadoGpio pin_configurar_entrada(PuertoDigital puerto, uint8_t pin)
{
    BloquePuertoGpio *gpio = obtener_bloque_puerto(puerto);

    if (gpio == (BloquePuertoGpio *)0)
    {
        return GPIO_ERROR_PUERTO;
    }

    if (!pin_es_valido(pin))
    {
        return GPIO_ERROR_PIN;
    }

    (void)puerto_habilitar(puerto);

    /* MODER = 00: entrada digital. */
    escribir_campo_2_bits(&gpio->MODER, pin, VALOR_MODO_ENTRADA);

    /* PUPDR = 00: sin pull-up ni pull-down. */
    escribir_campo_2_bits(&gpio->PUPDR, pin, VALOR_SIN_RESISTENCIA);

    return GPIO_EXITO;
}

EstadoGpio pin_configurar_salida(PuertoDigital puerto, uint8_t pin)
{
    BloquePuertoGpio *gpio = obtener_bloque_puerto(puerto);

    if (gpio == (BloquePuertoGpio *)0)
    {
        return GPIO_ERROR_PUERTO;
    }

    if (!pin_es_valido(pin))
    {
        return GPIO_ERROR_PIN;
    }

    (void)puerto_habilitar(puerto);

    /* MODER = 01: salida digital. */
    escribir_campo_2_bits(&gpio->MODER, pin, VALOR_MODO_SALIDA);

    /* OTYPER = 0: salida push-pull. */
    gpio->OTYPER &= ~BIT_GPIO(pin);

    /* OSPEEDR = 00: velocidad baja. */
    escribir_campo_2_bits(&gpio->OSPEEDR, pin, VALOR_VELOCIDAD_BAJA);

    /* PUPDR = 00: sin resistencia interna. */
    escribir_campo_2_bits(&gpio->PUPDR, pin, VALOR_SIN_RESISTENCIA);

    return GPIO_EXITO;
}

EstadoGpio pin_leer(PuertoDigital puerto,
                    uint8_t pin,
                    NivelDigital *nivel)
{
    BloquePuertoGpio *gpio = obtener_bloque_puerto(puerto);

    if (gpio == (BloquePuertoGpio *)0)
    {
        return GPIO_ERROR_PUERTO;
    }

    if (!pin_es_valido(pin))
    {
        return GPIO_ERROR_PIN;
    }

    if (nivel == (NivelDigital *)0)
    {
        return GPIO_ERROR_PUNTERO;
    }

    /* Acceso real a IDR para leer el estado fisico del pin. */
    *nivel = ((gpio->IDR & BIT_GPIO(pin)) != 0UL)
               ? NIVEL_ALTO
               : NIVEL_BAJO;

    return GPIO_EXITO;
}

EstadoGpio pin_escribir(PuertoDigital puerto,
                        uint8_t pin,
                        NivelDigital nivel)
{
    BloquePuertoGpio *gpio = obtener_bloque_puerto(puerto);

    if (gpio == (BloquePuertoGpio *)0)
    {
        return GPIO_ERROR_PUERTO;
    }

    if (!pin_es_valido(pin))
    {
        return GPIO_ERROR_PIN;
    }

    if ((nivel != NIVEL_BAJO) && (nivel != NIVEL_ALTO))
    {
        return GPIO_ERROR_VALOR;
    }

    if (nivel == NIVEL_ALTO)
    {
        /* BSRR[15:0] coloca el pin en 1. */
        gpio->BSRR = BIT_GPIO(pin);
    }
    else
    {
        /* BSRR[31:16] coloca el pin en 0. */
        gpio->BSRR = BIT_GPIO((uint32_t)pin + 16UL);
    }

    return GPIO_EXITO;
}

EstadoGpio pin_alternar(PuertoDigital puerto, uint8_t pin)
{
    BloquePuertoGpio *gpio = obtener_bloque_puerto(puerto);
    NivelDigital nuevo_nivel;

    if (gpio == (BloquePuertoGpio *)0)
    {
        return GPIO_ERROR_PUERTO;
    }

    if (!pin_es_valido(pin))
    {
        return GPIO_ERROR_PIN;
    }

    /* ODR informa el estado almacenado de la salida. */
    nuevo_nivel = ((gpio->ODR & BIT_GPIO(pin)) != 0UL)
                    ? NIVEL_BAJO
                    : NIVEL_ALTO;

    return pin_escribir(puerto, pin, nuevo_nivel);
}

EstadoGpio puerto_leer_bits(PuertoDigital puerto,
                            uint16_t mascara,
                            uint16_t *valor)
{
    BloquePuertoGpio *gpio = obtener_bloque_puerto(puerto);

    if (gpio == (BloquePuertoGpio *)0)
    {
        return GPIO_ERROR_PUERTO;
    }

    if (mascara == 0U)
    {
        return GPIO_ERROR_MASCARA;
    }

    if (valor == (uint16_t *)0)
    {
        return GPIO_ERROR_PUNTERO;
    }

    /* Opcion A: la mascara selecciona los bits que se desean leer. */
    *valor = (uint16_t)(gpio->IDR & (uint32_t)mascara);

    return GPIO_EXITO;
}

EstadoGpio puerto_escribir_bits(PuertoDigital puerto,
                                uint16_t mascara,
                                uint16_t valor)
{
    BloquePuertoGpio *gpio = obtener_bloque_puerto(puerto);
    uint32_t bits_poner;
    uint32_t bits_limpiar;

    if (gpio == (BloquePuertoGpio *)0)
    {
        return GPIO_ERROR_PUERTO;
    }

    if (mascara == 0U)
    {
        return GPIO_ERROR_MASCARA;
    }

    /*
     * Opcion A: mascara y valor.
     * - bits_poner contiene los bits seleccionados que deben quedar en 1.
     * - bits_limpiar contiene los bits seleccionados que deben quedar en 0.
     * Los bits fuera de la mascara no aparecen en ninguna de las dos partes.
     */
    bits_poner = ((uint32_t)valor & (uint32_t)mascara) &
                 MASCARA_PUERTO_GPIO;

    bits_limpiar = ((~(uint32_t)valor) & (uint32_t)mascara) &
                    MASCARA_PUERTO_GPIO;

    /* Una sola escritura en BSRR modifica solamente los bits seleccionados. */
    gpio->BSRR = bits_poner | (bits_limpiar << 16U);

    return GPIO_EXITO;
}
