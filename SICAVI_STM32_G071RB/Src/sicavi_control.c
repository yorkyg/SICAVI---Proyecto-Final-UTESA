#include "sicavi_control.h"
#include "horometro.h"
#include "ldr.h"
#include "protocolo_uart.h"
#include "sicavi_config.h"

#define GPIO_OMITIR_CONSTANTES_PIN
#include "gpio.h"

#include <stdio.h>
#include <string.h>

typedef enum
{
    SICAVI_DETENIDO = 0,
    SICAVI_TRANSPORTANDO,
    SICAVI_POSICIONANDO,
    SICAVI_ESTABILIZANDO,
    SICAVI_ESPERANDO_RESULTADO,
    SICAVI_DEJANDO_PASAR,
    SICAVI_RECHAZANDO,
    SICAVI_ALARMA
} EstadoSicavi;

typedef struct
{
    uint8_t estable;
    uint8_t ultima_muestra;
    uint8_t contador;
} EntradaAntirrebote;

static volatile EstadoSicavi estado_actual;
static HorometroSicavi horometro;
static MedicionLdr ldr;
static EntradaAntirrebote boton_start;
static EntradaAntirrebote boton_stop;
static EntradaAntirrebote sensor_caja;

static volatile uint16_t ticks_pendientes;
static volatile uint8_t deteccion_sensor_pendiente;
static uint32_t tick_actual;
static uint16_t divisor_segundo;
static uint16_t divisor_ldr;
static uint16_t divisor_telemetria;
static uint32_t vencimiento_estado;
static uint32_t vencimiento_vision;
static uint32_t ultima_comunicacion_ms;
static uint32_t contador_cajas;
static volatile uint8_t motor_activo;
static uint8_t expulsor_activo;
static uint8_t muestras_luz_invalida;
static uint8_t parpadeo_alarma;
static char id_activo[16];
static char resultado_activo[16];
static char codigo_alarma[32];

static const char *estado_texto(void);
static uint8_t tiempo_cumplido(uint32_t vencimiento);
static uint8_t entrada_activa(PuertoDigital puerto, uint8_t pin, uint8_t activa_bajo);
static uint8_t antirrebote_actualizar(EntradaAntirrebote *entrada, uint8_t muestra);
static void salidas_inicializar(void);
static void motor_escribir(uint8_t activo);
static void expulsor_escribir(uint8_t activo);
static void leds_actualizar(void);
static void detener_sistema(const char *origen);
static void iniciar_sistema(const char *origen);
static void activar_alarma(const char *codigo);
static void iniciar_posicionamiento_caja(void);
static void detectar_caja(void);
static void aceptar_resultado(const char *resultado, const char *id);
static void procesar_comando(const char *linea);
static const char *campo_id(const char *linea);
static void enviar_telemetria(void);
static void procesar_tick_10ms(void);

void sicavi_inicializar(UART_HandleTypeDef *huart)
{
    ticks_pendientes = 0U;
    deteccion_sensor_pendiente = 0U;
    tick_actual = 0UL;
    divisor_segundo = 0U;
    divisor_ldr = 0U;
    divisor_telemetria = 0U;
    vencimiento_estado = 0UL;
    vencimiento_vision = 0UL;
    ultima_comunicacion_ms = HAL_GetTick();
    contador_cajas = 0UL;
    motor_activo = 0U;
    expulsor_activo = 0U;
    muestras_luz_invalida = 0U;
    parpadeo_alarma = 0U;
    id_activo[0] = '\0';
    resultado_activo[0] = '\0';
    codigo_alarma[0] = '\0';

    horometro_inicializar(&horometro);
    salidas_inicializar();

    boton_start.estable = entrada_activa(PUERTO_C, SICAVI_PIN_START_PC13, SICAVI_START_ACTIVO_BAJO);
    boton_start.ultima_muestra = boton_start.estable;
    boton_start.contador = 0U;
    boton_stop.estable = entrada_activa(PUERTO_C, SICAVI_PIN_STOP_PC1, SICAVI_STOP_ACTIVO_BAJO);
    boton_stop.ultima_muestra = boton_stop.estable;
    boton_stop.contador = 0U;
    sensor_caja.estable = entrada_activa(PUERTO_A, SICAVI_PIN_SENSOR_PA9_D8, !SICAVI_SENSOR_ACTIVO_ALTO);
    sensor_caja.ultima_muestra = sensor_caja.estable;
    sensor_caja.contador = 0U;

    (void)ldr_inicializar();
    (void)ldr_actualizar(&ldr);
    (void)protocolo_uart_inicializar(huart);

    estado_actual = SICAVI_DETENIDO;
    motor_escribir(0U);
    expulsor_escribir(0U);
    leds_actualizar();

    protocolo_uart_enviar("EVT=BOOT;FW=" SICAVI_FIRMWARE_VERSION ";BOARD=NUCLEO_G071RB\r\n");
    enviar_telemetria();
}

void sicavi_tick_10ms_isr(void)
{
    if (ticks_pendientes < 1000U) ticks_pendientes++;
}

void sicavi_sensor_caja_isr(void)
{
    /*
     * D8 reserva la caja una sola vez. El motor continua durante el avance
     * configurado para centrarla; temporizacion y UART quedan en el lazo.
     */
    if ((estado_actual == SICAVI_TRANSPORTANDO) && (deteccion_sensor_pendiente == 0U))
    {
        estado_actual = SICAVI_POSICIONANDO;
        deteccion_sensor_pendiente = 1U;
    }
}

void sicavi_procesar(void)
{
    char linea[128];
    uint8_t procesar_deteccion = 0U;

    __disable_irq();
    if (deteccion_sensor_pendiente != 0U)
    {
        deteccion_sensor_pendiente = 0U;
        procesar_deteccion = 1U;
    }
    __enable_irq();

    if (procesar_deteccion != 0U)
    {
        iniciar_posicionamiento_caja();
    }

    if (protocolo_uart_procesar() != 0U)
    {
        activar_alarma("UART_ERROR");
    }

    protocolo_uart_supervisar_recepcion();

    while (protocolo_uart_extraer_linea(linea, sizeof(linea)) != 0U)
    {
        procesar_comando(linea);
    }

    for (;;)
    {
        uint8_t hay_tick = 0U;
        __disable_irq();
        if (ticks_pendientes > 0U)
        {
            ticks_pendientes--;
            hay_tick = 1U;
        }
        __enable_irq();

        if (hay_tick == 0U) break;
        procesar_tick_10ms();
    }
}

static void procesar_tick_10ms(void)
{
    uint8_t evento_start;
    uint8_t evento_stop;
    uint8_t evento_sensor;

    tick_actual++;

    evento_start = antirrebote_actualizar(
        &boton_start,
        entrada_activa(PUERTO_C, SICAVI_PIN_START_PC13, SICAVI_START_ACTIVO_BAJO));
    evento_stop = antirrebote_actualizar(
        &boton_stop,
        entrada_activa(PUERTO_C, SICAVI_PIN_STOP_PC1, SICAVI_STOP_ACTIVO_BAJO));
    evento_sensor = antirrebote_actualizar(
        &sensor_caja,
        entrada_activa(PUERTO_A, SICAVI_PIN_SENSOR_PA9_D8, !SICAVI_SENSOR_ACTIVO_ALTO));

    if (evento_stop != 0U)
    {
        detener_sistema("LOCAL");
    }
    else if ((evento_start != 0U) && (estado_actual == SICAVI_DETENIDO))
    {
        iniciar_sistema("LOCAL");
    }

    if ((evento_sensor != 0U) && (estado_actual == SICAVI_TRANSPORTANDO))
    {
        /* Respaldo por antirrebote si el flanco EXTI no se atendio. */
        sicavi_sensor_caja_isr();
    }

    divisor_ldr++;
    if (divisor_ldr >= SICAVI_MS_A_TICKS(SICAVI_MUESTREO_LDR_MS))
    {
        divisor_ldr = 0U;
        if (ldr_actualizar(&ldr) != HAL_OK)
        {
            if (estado_actual != SICAVI_DETENIDO) activar_alarma("ADC_ERROR");
        }
        else if ((estado_actual != SICAVI_DETENIDO) && (estado_actual != SICAVI_ALARMA))
        {
#if SICAVI_BLOQUEO_POR_LUZ_HABILITADO
            if ((ldr.porcentaje < SICAVI_LUZ_MINIMA_PORCENTAJE) ||
                (ldr.porcentaje > SICAVI_LUZ_MAXIMA_PORCENTAJE))
            {
                if (muestras_luz_invalida < 255U) muestras_luz_invalida++;
                if (muestras_luz_invalida >= 5U) activar_alarma("LIGHT_OUT_OF_RANGE");
            }
            else
            {
                muestras_luz_invalida = 0U;
            }
#else
            /* El LDR se conserva como diagnostico, pero no detiene la banda. */
            muestras_luz_invalida = 0U;
#endif
        }
    }

    divisor_segundo++;
    if (divisor_segundo >= SICAVI_MS_A_TICKS(1000UL))
    {
        divisor_segundo = 0U;
        horometro_tick_1s(&horometro, motor_activo);
    }

    divisor_telemetria++;
    if (divisor_telemetria >= SICAVI_MS_A_TICKS(SICAVI_TELEMETRIA_MS))
    {
        divisor_telemetria = 0U;
        enviar_telemetria();
    }

    if ((estado_actual == SICAVI_ESPERANDO_RESULTADO) && tiempo_cumplido(vencimiento_vision))
    {
        activar_alarma("VISION_TIMEOUT");
    }
    else if ((estado_actual != SICAVI_DETENIDO) &&
             (estado_actual != SICAVI_ALARMA) &&
             ((uint32_t)(HAL_GetTick() - ultima_comunicacion_ms) >= SICAVI_TIMEOUT_COMUNICACION_MS))
    {
        activar_alarma("COMM_TIMEOUT");
    }
    else if ((estado_actual == SICAVI_POSICIONANDO) && tiempo_cumplido(vencimiento_estado))
    {
        motor_escribir(0U);
        estado_actual = SICAVI_ESTABILIZANDO;
        vencimiento_estado = tick_actual + SICAVI_MS_A_TICKS(SICAVI_ESTABILIZACION_IMAGEN_MS);
        leds_actualizar();
    }
    else if ((estado_actual == SICAVI_ESTABILIZANDO) && tiempo_cumplido(vencimiento_estado))
    {
        detectar_caja();
    }
    else if ((estado_actual == SICAVI_DEJANDO_PASAR) && tiempo_cumplido(vencimiento_estado))
    {
        protocolo_uart_enviar("EVT=FINISHED;ID=%s;RESULT=GOOD\r\n", id_activo);
        estado_actual = SICAVI_TRANSPORTANDO;
        motor_escribir(1U);
        leds_actualizar();
    }
    else if ((estado_actual == SICAVI_RECHAZANDO) && tiempo_cumplido(vencimiento_estado))
    {
        expulsor_escribir(0U);
        protocolo_uart_enviar("EVT=FINISHED;ID=%s;RESULT=REJECT\r\n", id_activo);
        estado_actual = SICAVI_TRANSPORTANDO;
        motor_escribir(1U);
        leds_actualizar();
    }

    if ((estado_actual == SICAVI_ALARMA) && ((tick_actual % SICAVI_MS_A_TICKS(500UL)) == 0UL))
    {
        parpadeo_alarma = (uint8_t)!parpadeo_alarma;
        (void)pin_escribir(PUERTO_A, SICAVI_PIN_LED_ESTADO_PA7,
                           parpadeo_alarma ? NIVEL_ALTO : NIVEL_BAJO);
    }
}

static void salidas_inicializar(void)
{
    GPIO_InitTypeDef sensor_gpio = {0};
    GPIO_InitTypeDef botones_gpio = {0};

    (void)pin_configurar_salida(PUERTO_A, SICAVI_PIN_LED_ACEPTADO_PA5);
    (void)pin_configurar_salida(PUERTO_A, SICAVI_PIN_LED_RECHAZADO_PA6);
    (void)pin_configurar_salida(PUERTO_A, SICAVI_PIN_LED_ESTADO_PA7);
    (void)pin_configurar_salida(PUERTO_C, SICAVI_PIN_MOTOR_PC7_D9);
    (void)pin_configurar_salida(PUERTO_B, SICAVI_PIN_EXPULSOR_PB1);
    (void)pin_configurar_entrada(PUERTO_A, SICAVI_PIN_SENSOR_PA9_D8);
    (void)pin_configurar_entrada(PUERTO_C, SICAVI_PIN_STOP_PC1);
    (void)pin_configurar_entrada(PUERTO_C, SICAVI_PIN_START_PC13);

    /*
     * START y STOP son entradas activas en bajo. El pull-up evita arranques o
     * paradas aleatorias si un pulsador/cable queda momentaneamente abierto.
     * PC13 conserva ademas el pulsador B1 integrado de la NUCLEO.
     */
    botones_gpio.Pin = GPIO_PIN_1 | GPIO_PIN_13;
    botones_gpio.Mode = GPIO_MODE_INPUT;
    botones_gpio.Pull = GPIO_PULLUP;
    HAL_GPIO_Init(GPIOC, &botones_gpio);

    /* D8/PA9 inicia el posicionamiento temporizado sin esperar el antirrebote. */
    sensor_gpio.Pin = GPIO_PIN_9;
#if SICAVI_SENSOR_ACTIVO_ALTO
    sensor_gpio.Mode = GPIO_MODE_IT_RISING;
#else
    sensor_gpio.Mode = GPIO_MODE_IT_FALLING;
#endif
#if SICAVI_SENSOR_ACTIVO_ALTO
    sensor_gpio.Pull = GPIO_PULLDOWN;
#else
    sensor_gpio.Pull = GPIO_PULLUP;
#endif
    HAL_GPIO_Init(GPIOA, &sensor_gpio);
    HAL_NVIC_SetPriority(EXTI4_15_IRQn, 0U, 0U);
    HAL_NVIC_EnableIRQ(EXTI4_15_IRQn);

    /* Garantiza que las bobinas queden inactivas desde el primer momento. */
    motor_escribir(0U);
    expulsor_escribir(0U);
}

static uint8_t entrada_activa(PuertoDigital puerto, uint8_t pin, uint8_t activa_bajo)
{
    NivelDigital nivel = (activa_bajo != 0U) ? NIVEL_ALTO : NIVEL_BAJO;
    if (pin_leer(puerto, pin, &nivel) != GPIO_EXITO) return 0U;
    if (activa_bajo != 0U) return (nivel == NIVEL_BAJO) ? 1U : 0U;
    return (nivel == NIVEL_ALTO) ? 1U : 0U;
}

static uint8_t antirrebote_actualizar(EntradaAntirrebote *entrada, uint8_t muestra)
{
    const uint8_t muestras_requeridas = (uint8_t)SICAVI_MS_A_TICKS(SICAVI_ANTIRREBOTE_MS);

    if (muestra != entrada->ultima_muestra)
    {
        entrada->ultima_muestra = muestra;
        entrada->contador = 0U;
        return 0U;
    }

    if (entrada->contador < muestras_requeridas) entrada->contador++;
    if ((entrada->contador >= muestras_requeridas) && (entrada->estable != muestra))
    {
        entrada->estable = muestra;
        return (muestra != 0U) ? 1U : 0U;
    }
    return 0U;
}

static void motor_escribir(uint8_t activo)
{
    motor_activo = (activo != 0U) ? 1U : 0U;
    (void)pin_escribir(PUERTO_C, SICAVI_PIN_MOTOR_PC7_D9,
        (motor_activo == SICAVI_DRIVER_MOTOR_ACTIVO_ALTO) ? NIVEL_ALTO : NIVEL_BAJO);
}

static void expulsor_escribir(uint8_t activo)
{
    uint8_t nuevo = (activo != 0U) ? 1U : 0U;
    if ((nuevo != 0U) && (expulsor_activo == 0U))
    {
        horometro_registrar_ciclo_expulsor(&horometro);
    }
    expulsor_activo = nuevo;
    (void)pin_escribir(PUERTO_B, SICAVI_PIN_EXPULSOR_PB1,
        (expulsor_activo == SICAVI_RELE_EXPULSOR_ACTIVO_ALTO) ? NIVEL_ALTO : NIVEL_BAJO);
}

static void leds_actualizar(void)
{
    (void)pin_escribir(PUERTO_A, SICAVI_PIN_LED_ACEPTADO_PA5, NIVEL_BAJO);
    (void)pin_escribir(PUERTO_A, SICAVI_PIN_LED_RECHAZADO_PA6, NIVEL_BAJO);
    (void)pin_escribir(PUERTO_A, SICAVI_PIN_LED_ESTADO_PA7, NIVEL_BAJO);

    if (estado_actual == SICAVI_DETENIDO)
    {
        /* STOP: los tres indicadores quedan apagados. */
        return;
    }

    if (estado_actual == SICAVI_ALARMA)
    {
        parpadeo_alarma = 1U;
        (void)pin_escribir(PUERTO_A, SICAVI_PIN_LED_ESTADO_PA7, NIVEL_ALTO);
        return;
    }

    /* RUN: amarillo fijo. Verde o rojo indican la ultima decision. */
    (void)pin_escribir(PUERTO_A, SICAVI_PIN_LED_ESTADO_PA7, NIVEL_ALTO);

    if (strcmp(resultado_activo, "GOOD") == 0)
    {
        (void)pin_escribir(PUERTO_A, SICAVI_PIN_LED_ACEPTADO_PA5, NIVEL_ALTO);
    }
    else if (strcmp(resultado_activo, "REJECT") == 0)
    {
        (void)pin_escribir(PUERTO_A, SICAVI_PIN_LED_RECHAZADO_PA6, NIVEL_ALTO);
    }
}

static void detener_sistema(const char *origen)
{
    deteccion_sensor_pendiente = 0U;
    estado_actual = SICAVI_DETENIDO;
    motor_escribir(0U);
    expulsor_escribir(0U);
    id_activo[0] = '\0';
    resultado_activo[0] = '\0';
    leds_actualizar();
    protocolo_uart_enviar("EVT=STOPPED;SOURCE=%s\r\n", origen);
    protocolo_uart_enviar("ACK=STOP;STATE=STOPPED\r\n");
}

static void iniciar_sistema(const char *origen)
{
    if ((estado_actual == SICAVI_ALARMA) || (codigo_alarma[0] != '\0'))
    {
        protocolo_uart_enviar("ERR=RUN;CODE=RESET_REQUIRED\r\n");
        return;
    }

    /*
     * Un sensor activo al recibir RUN no produce un nuevo flanco descendente
     * y antes hacia que el motor pareciera no arrancar. Se rechaza el comando
     * de forma explicita para distinguir caja presente, sensor bloqueado o
     * cableado en corto de una alarma retenida.
     */
    if (entrada_activa(PUERTO_A, SICAVI_PIN_SENSOR_PA9_D8,
                       !SICAVI_SENSOR_ACTIVO_ALTO) != 0U)
    {
        protocolo_uart_enviar("ERR=RUN;CODE=BOX_SENSOR_ACTIVE\r\n");
        enviar_telemetria();
        return;
    }

#if SICAVI_BLOQUEO_POR_LUZ_HABILITADO
    if ((ldr.valida == 0U) ||
        (ldr.porcentaje < SICAVI_LUZ_MINIMA_PORCENTAJE) ||
        (ldr.porcentaje > SICAVI_LUZ_MAXIMA_PORCENTAJE))
    {
        activar_alarma("LIGHT_OUT_OF_RANGE");
        protocolo_uart_enviar("ERR=RUN;CODE=LIGHT_OUT_OF_RANGE\r\n");
        return;
    }
#endif

    estado_actual = SICAVI_TRANSPORTANDO;
    ultima_comunicacion_ms = HAL_GetTick();
    muestras_luz_invalida = 0U;
    resultado_activo[0] = '\0';
    motor_escribir(1U);
    expulsor_escribir(0U);
    leds_actualizar();
    protocolo_uart_enviar("EVT=RUNNING;SOURCE=%s\r\n", origen);
    protocolo_uart_enviar("ACK=RUN;STATE=TRANSPORTING\r\n");
}

static void activar_alarma(const char *codigo)
{
    if (estado_actual == SICAVI_ALARMA) return;
    estado_actual = SICAVI_ALARMA;
    motor_escribir(0U);
    expulsor_escribir(0U);
    snprintf(codigo_alarma, sizeof(codigo_alarma), "%s", codigo);
    leds_actualizar();
    protocolo_uart_enviar("EVT=ALARM;CODE=%s\r\n", codigo_alarma);
}

static void iniciar_posicionamiento_caja(void)
{
    if (estado_actual != SICAVI_POSICIONANDO) return;

    motor_escribir(1U);
    vencimiento_estado = tick_actual + SICAVI_MS_A_TICKS(SICAVI_AVANCE_TRAS_SENSOR_MS);
    leds_actualizar();
}

static void detectar_caja(void)
{
    contador_cajas++;
    snprintf(id_activo, sizeof(id_activo), "%lu", (unsigned long)contador_cajas);
    resultado_activo[0] = '\0';
    estado_actual = SICAVI_ESPERANDO_RESULTADO;
    motor_escribir(0U);
    vencimiento_vision = tick_actual + SICAVI_MS_A_TICKS(SICAVI_TIMEOUT_VISION_MS);
    leds_actualizar();
    protocolo_uart_enviar("EVT=DETECTED;ID=%s;LIGHT=%u;ADC=%u\r\n",
                          id_activo, ldr.porcentaje, ldr.adc);
}

static void aceptar_resultado(const char *resultado, const char *id)
{
    if (estado_actual != SICAVI_ESPERANDO_RESULTADO)
    {
        protocolo_uart_enviar("ERR=%s;CODE=NOT_WAITING\r\n", resultado);
        return;
    }
    if ((id == 0) || (strcmp(id, id_activo) != 0))
    {
        protocolo_uart_enviar("ERR=%s;CODE=ID_MISMATCH\r\n", resultado);
        return;
    }

    snprintf(resultado_activo, sizeof(resultado_activo), "%s", resultado);

    if (strcmp(resultado, "GOOD") == 0)
    {
        estado_actual = SICAVI_DEJANDO_PASAR;
        motor_escribir(1U);
        vencimiento_estado = tick_actual + SICAVI_MS_A_TICKS(SICAVI_TIEMPO_PASO_MS);
        leds_actualizar();
        protocolo_uart_enviar("ACK=GOOD;ID=%s\r\n", id_activo);
    }
    else if (strcmp(resultado, "REJECT") == 0)
    {
        estado_actual = SICAVI_RECHAZANDO;
        motor_escribir(0U);
        expulsor_escribir(1U);
        vencimiento_estado = tick_actual + SICAVI_MS_A_TICKS(SICAVI_TIEMPO_EXPULSOR_MS);
        leds_actualizar();
        protocolo_uart_enviar("ACK=REJECT;ID=%s\r\n", id_activo);
    }
    else
    {
        protocolo_uart_enviar("ACK=UNKNOWN;ID=%s\r\n", id_activo);
        activar_alarma("VISION_UNKNOWN");
    }
}

static const char *campo_id(const char *linea)
{
    const char *inicio = strstr(linea, ";ID=");
    if (inicio == 0) return 0;
    return inicio + 4;
}

static void procesar_comando(const char *linea)
{
    ultima_comunicacion_ms = HAL_GetTick();

    if (strcmp(linea, "CMD=RUN") == 0)
    {
        iniciar_sistema("C_SHARP");
    }
    else if (strcmp(linea, "CMD=STOP") == 0)
    {
        detener_sistema("C_SHARP");
    }
    else if (strcmp(linea, "CMD=RESET") == 0)
    {
        deteccion_sensor_pendiente = 0U;
        estado_actual = SICAVI_DETENIDO;
        motor_escribir(0U);
        expulsor_escribir(0U);
        vencimiento_estado = 0UL;
        vencimiento_vision = 0UL;
        codigo_alarma[0] = '\0';
        id_activo[0] = '\0';
        resultado_activo[0] = '\0';
        muestras_luz_invalida = 0U;
        parpadeo_alarma = 0U;
        leds_actualizar();
        protocolo_uart_enviar("ACK=RESET;STATE=STOPPED;ALARM=NONE\r\n");
        enviar_telemetria();
    }
    else if (strcmp(linea, "CMD=STATUS") == 0)
    {
        enviar_telemetria();
        protocolo_uart_enviar("ACK=STATUS\r\n");
    }
    else if (strcmp(linea, "CMD=PING") == 0)
    {
        /* El latido solo renueva el temporizador; no genera trafico de respuesta. */
    }
    else if (strncmp(linea, "CMD=GOOD;ID=", 12U) == 0)
    {
        aceptar_resultado("GOOD", campo_id(linea));
    }
    else if (strncmp(linea, "CMD=REJECT;ID=", 14U) == 0)
    {
        aceptar_resultado("REJECT", campo_id(linea));
    }
    else if (strncmp(linea, "CMD=UNKNOWN;ID=", 15U) == 0)
    {
        aceptar_resultado("UNKNOWN", campo_id(linea));
    }
    else
    {
        protocolo_uart_enviar("ERR=COMMAND;CODE=UNKNOWN\r\n");
    }
}

static void enviar_telemetria(void)
{
    protocolo_uart_enviar(
        "TEL=STATUS;FW=" SICAVI_FIRMWARE_VERSION ";STATE=%s;MOTOR=%u;BOX_SENSOR=%u;LIGHT=%u;ADC=%u;MV=%u;HOURMETER_S=%lu;CYCLES=%lu;ALARM=%s\r\n",
        estado_texto(),
        motor_activo,
        entrada_activa(PUERTO_A, SICAVI_PIN_SENSOR_PA9_D8, !SICAVI_SENSOR_ACTIVO_ALTO),
        ldr.porcentaje,
        ldr.adc,
        ldr.milivoltios,
        (unsigned long)horometro.segundos_trabajo,
        (unsigned long)horometro.ciclos_expulsor,
        (codigo_alarma[0] != '\0') ? codigo_alarma : "NONE");
}

static const char *estado_texto(void)
{
    switch (estado_actual)
    {
        case SICAVI_DETENIDO: return "STOPPED";
        case SICAVI_TRANSPORTANDO: return "TRANSPORTING";
        case SICAVI_POSICIONANDO: return "POSITIONING";
        case SICAVI_ESTABILIZANDO: return "STABILIZING";
        case SICAVI_ESPERANDO_RESULTADO: return "WAITING_RESULT";
        case SICAVI_DEJANDO_PASAR: return "PASSING";
        case SICAVI_RECHAZANDO: return "REJECTING";
        case SICAVI_ALARMA: return "ALARM";
        default: return "UNKNOWN";
    }
}

static uint8_t tiempo_cumplido(uint32_t vencimiento)
{
    return ((int32_t)(tick_actual - vencimiento) >= 0) ? 1U : 0U;
}
