#ifndef SICAVI_CONTROL_H_
#define SICAVI_CONTROL_H_

#include "stm32g0xx_hal.h"

void sicavi_inicializar(UART_HandleTypeDef *huart);
void sicavi_procesar(void);
void sicavi_tick_10ms_isr(void);
void sicavi_sensor_caja_isr(void);

#endif /* SICAVI_CONTROL_H_ */
