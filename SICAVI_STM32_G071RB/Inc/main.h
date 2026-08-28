#ifndef MAIN_H_
#define MAIN_H_

#ifdef __cplusplus
extern "C" {
#endif

#include "stm32g0xx_hal.h"
#include "stm32g0xx_ll_system.h"

void Error_Handler(void);

#define USART2_TX_Pin        GPIO_PIN_2
#define USART2_TX_GPIO_Port  GPIOA
#define USART2_RX_Pin        GPIO_PIN_3
#define USART2_RX_GPIO_Port  GPIOA

#ifdef __cplusplus
}
#endif

#endif /* MAIN_H_ */
