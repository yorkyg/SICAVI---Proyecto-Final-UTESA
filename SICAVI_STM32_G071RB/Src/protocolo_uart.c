#include "protocolo_uart.h"
#include <stdarg.h>
#include <stdio.h>
#include <string.h>

#define UART_LINEA_CAPACIDAD  128U
#define UART_TX_CAPACIDAD     256U
#define UART_COLA_CAPACIDAD   8U
#define UART_RX_VIGILANCIA_MS 1800UL
#define UART_RX_REINTENTO_MS  250UL

static UART_HandleTypeDef *uart_sicavi;
static uint8_t rx_byte;
static volatile uint16_t rx_indice;
static volatile uint8_t cola_lectura;
static volatile uint8_t cola_escritura;
static volatile uint8_t lineas_pendientes;
static volatile uint8_t error_pendiente;
static volatile uint32_t ultimo_byte_rx_ms;
static uint32_t ultimo_reinicio_rx_ms;
static char rx_armado[UART_LINEA_CAPACIDAD];
static char rx_cola[UART_COLA_CAPACIDAD][UART_LINEA_CAPACIDAD];

static HAL_StatusTypeDef reiniciar_recepcion(void);

HAL_StatusTypeDef protocolo_uart_inicializar(UART_HandleTypeDef *huart)
{
    if (huart == 0) return HAL_ERROR;
    uart_sicavi = huart;
    rx_indice = 0U;
    cola_lectura = 0U;
    cola_escritura = 0U;
    lineas_pendientes = 0U;
    error_pendiente = 0U;
    ultimo_byte_rx_ms = HAL_GetTick();
    ultimo_reinicio_rx_ms = ultimo_byte_rx_ms;
    return HAL_UART_Receive_IT(uart_sicavi, &rx_byte, 1U);
}

void protocolo_uart_enviar(const char *formato, ...)
{
    char salida[UART_TX_CAPACIDAD];
    int longitud;
    va_list argumentos;

    if ((uart_sicavi == 0) || (formato == 0)) return;

    va_start(argumentos, formato);
    longitud = vsnprintf(salida, sizeof(salida), formato, argumentos);
    va_end(argumentos);

    if (longitud <= 0) return;
    if ((size_t)longitud >= sizeof(salida)) longitud = (int)sizeof(salida) - 1;

    (void)HAL_UART_Transmit(uart_sicavi, (uint8_t *)salida, (uint16_t)longitud, 100U);
}

uint8_t protocolo_uart_extraer_linea(char *destino, size_t capacidad)
{
    size_t longitud = 0U;
    uint8_t indice_lectura;

    if ((destino == 0) || (capacidad == 0U) || (lineas_pendientes == 0U)) return 0U;

    __disable_irq();
    indice_lectura = cola_lectura;
    while ((longitud < UART_LINEA_CAPACIDAD) &&
           (rx_cola[indice_lectura][longitud] != '\0')) longitud++;
    if (longitud >= capacidad) longitud = capacidad - 1U;
    memcpy(destino, rx_cola[indice_lectura], longitud);
    destino[longitud] = '\0';
    cola_lectura = (uint8_t)((cola_lectura + 1U) % UART_COLA_CAPACIDAD);
    lineas_pendientes--;
    __enable_irq();
    return 1U;
}

uint8_t protocolo_uart_procesar(void)
{
    if (error_pendiente != 0U)
    {
        error_pendiente = 0U;
        return (reiniciar_recepcion() == HAL_OK) ? 0U : 1U;
    }
    return 0U;
}

void protocolo_uart_supervisar_recepcion(void)
{
    uint32_t ahora;

    if (uart_sicavi == 0) return;

    ahora = HAL_GetTick();
    if (((uint32_t)(ahora - ultimo_byte_rx_ms) >= UART_RX_VIGILANCIA_MS) &&
        ((uint32_t)(ahora - ultimo_reinicio_rx_ms) >= UART_RX_REINTENTO_MS))
    {
        /*
         * El ST-LINK VCP puede dejar la recepcion HAL sin interrupcion activa
         * tras ruido, reconexion USB u overrun. Se rearma antes del timeout de
         * comunicacion para que el siguiente PING o RESET vuelva a entrar.
         */
        if (reiniciar_recepcion() != HAL_OK)
        {
            error_pendiente = 1U;
        }
    }
}

void protocolo_uart_rx_callback(UART_HandleTypeDef *huart)
{
    if ((uart_sicavi == 0) || (huart != uart_sicavi)) return;

    ultimo_byte_rx_ms = HAL_GetTick();

    if (rx_byte == (uint8_t)'\n')
    {
        if (rx_indice > 0U)
        {
            rx_armado[rx_indice] = '\0';
            if (lineas_pendientes < UART_COLA_CAPACIDAD)
            {
                memcpy(rx_cola[cola_escritura], rx_armado, (size_t)rx_indice + 1U);
                cola_escritura = (uint8_t)((cola_escritura + 1U) % UART_COLA_CAPACIDAD);
                lineas_pendientes++;
            }
            else
            {
                /* La linea se descarta completa y se informa al lazo principal. */
                error_pendiente = 1U;
            }
        }
        rx_indice = 0U;
    }
    else if (rx_byte != (uint8_t)'\r')
    {
        if (rx_indice < (UART_LINEA_CAPACIDAD - 1U))
        {
            rx_armado[rx_indice++] = (char)rx_byte;
        }
        else
        {
            rx_indice = 0U;
            error_pendiente = 1U;
        }
    }

    (void)HAL_UART_Receive_IT(uart_sicavi, &rx_byte, 1U);
}

void protocolo_uart_error_callback(UART_HandleTypeDef *huart)
{
    if ((uart_sicavi == 0) || (huart != uart_sicavi)) return;
    error_pendiente = 1U;
}

static HAL_StatusTypeDef reiniciar_recepcion(void)
{
    HAL_StatusTypeDef estado;

    if (uart_sicavi == 0) return HAL_ERROR;

    (void)HAL_UART_AbortReceive(uart_sicavi);

    __disable_irq();
    rx_indice = 0U;
    __enable_irq();

    ultimo_reinicio_rx_ms = HAL_GetTick();
    ultimo_byte_rx_ms = ultimo_reinicio_rx_ms;
    estado = HAL_UART_Receive_IT(uart_sicavi, &rx_byte, 1U);
    return estado;
}
