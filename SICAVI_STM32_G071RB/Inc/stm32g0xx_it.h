#ifndef STM32G0XX_IT_H_
#define STM32G0XX_IT_H_

void NMI_Handler(void);
void HardFault_Handler(void);
void SVC_Handler(void);
void PendSV_Handler(void);
void SysTick_Handler(void);
void EXTI4_15_IRQHandler(void);
void TIM6_DAC_LPTIM1_IRQHandler(void);
void USART2_IRQHandler(void);

#endif /* STM32G0XX_IT_H_ */
