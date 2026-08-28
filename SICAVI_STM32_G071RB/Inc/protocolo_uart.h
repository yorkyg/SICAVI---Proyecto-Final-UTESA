#ifndef PROTOCOLO_UART_H_
#define PROTOCOLO_UART_H_

#include "stm32g0xx_hal.h"
#include <stddef.h>
#include <stdint.h>

HAL_StatusTypeDef protocolo_uart_inicializar(UART_HandleTypeDef *huart);
uint8_t protocolo_uart_procesar(void);
void protocolo_uart_supervisar_recepcion(void);
uint8_t protocolo_uart_extraer_linea(char *destino, size_t capacidad);
void protocolo_uart_enviar(const char *formato, ...);
void protocolo_uart_rx_callback(UART_HandleTypeDef *huart);
void protocolo_uart_error_callback(UART_HandleTypeDef *huart);

#endif /* PROTOCOLO_UART_H_ */
