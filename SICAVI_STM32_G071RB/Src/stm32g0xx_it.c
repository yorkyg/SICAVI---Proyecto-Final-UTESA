#include "main.h"
#include "stm32g0xx_it.h"

extern TIM_HandleTypeDef htim6;
extern UART_HandleTypeDef huart2;

void NMI_Handler(void) { }
void HardFault_Handler(void) { while (1) { } }
void SVC_Handler(void) { }
void PendSV_Handler(void) { }
void SysTick_Handler(void) { HAL_IncTick(); }

void EXTI4_15_IRQHandler(void)
{
    HAL_GPIO_EXTI_IRQHandler(GPIO_PIN_9);
}

void TIM6_DAC_LPTIM1_IRQHandler(void)
{
    HAL_TIM_IRQHandler(&htim6);
}

void USART2_IRQHandler(void)
{
    HAL_UART_IRQHandler(&huart2);
}
