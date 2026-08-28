#include "ldr.h"

#define LDR_MUESTRAS_PROMEDIO  8U
#define LDR_ADC_MAXIMO         4095UL
#define LDR_VREF_MV            3300UL
#define LDR_TIMEOUT_MS         5UL
#define LDR_ESTABILIZACION     2000UL

static HAL_StatusTypeDef esperar_bandera(uint32_t bandera)
{
    uint32_t inicio = HAL_GetTick();
    while ((ADC1->ISR & bandera) == 0UL)
    {
        if ((HAL_GetTick() - inicio) >= LDR_TIMEOUT_MS) return HAL_TIMEOUT;
    }
    return HAL_OK;
}

HAL_StatusTypeDef ldr_inicializar(void)
{
    volatile uint32_t espera;
    uint32_t inicio;

    __HAL_RCC_GPIOA_CLK_ENABLE();
    __HAL_RCC_ADC_CLK_ENABLE();

    /* PA0 en modo analogico, sin pull-up ni pull-down. */
    GPIOA->MODER |= (3UL << (0U * 2U));
    GPIOA->PUPDR &= ~(3UL << (0U * 2U));

    /* Reloj ADC sincrono PCLK/2: 64 MHz / 2 = 32 MHz. */
    ADC1->CFGR2 = (ADC1->CFGR2 & ~ADC_CFGR2_CKMODE_Msk) | ADC_CFGR2_CKMODE_0;
    ADC1->CFGR1 &= ~ADC_CFGR1_CONT;
    ADC1->SMPR = (ADC1->SMPR & ~ADC_SMPR_SMP1_Msk) |
                 ADC_SMPR_SMP1_1 | ADC_SMPR_SMP1_2; /* 79.5 ciclos */
    ADC1->CHSELR = ADC_CHSELR_CHSEL0;

    ADC1->CR |= ADC_CR_ADVREGEN;
    for (espera = 0UL; espera < LDR_ESTABILIZACION; espera++) __NOP();

    ADC1->CR |= ADC_CR_ADCAL;
    inicio = HAL_GetTick();
    while ((ADC1->CR & ADC_CR_ADCAL) != 0UL)
    {
        if ((HAL_GetTick() - inicio) >= LDR_TIMEOUT_MS) return HAL_TIMEOUT;
    }
    for (espera = 0UL; espera < 32UL; espera++) __NOP();

    ADC1->ISR = ADC_ISR_ADRDY;
    ADC1->CR |= ADC_CR_ADEN;
    return esperar_bandera(ADC_ISR_ADRDY);
}

HAL_StatusTypeDef ldr_actualizar(MedicionLdr *medicion)
{
    uint32_t suma = 0UL;
    uint32_t promedio;
    uint8_t i;

    if (medicion == 0) return HAL_ERROR;

    for (i = 0U; i < LDR_MUESTRAS_PROMEDIO; i++)
    {
        ADC1->ISR = ADC_ISR_EOC | ADC_ISR_EOS;
        ADC1->CR |= ADC_CR_ADSTART;
        if (esperar_bandera(ADC_ISR_EOC) != HAL_OK)
        {
            goto error_adc;
        }
        suma += (ADC1->DR & 0x0FFFUL);
    }

    promedio = (suma + (LDR_MUESTRAS_PROMEDIO / 2U)) / LDR_MUESTRAS_PROMEDIO;
    medicion->adc = (uint16_t)promedio;
    medicion->milivoltios = (uint16_t)((promedio * LDR_VREF_MV + (LDR_ADC_MAXIMO / 2UL)) / LDR_ADC_MAXIMO);
    medicion->porcentaje = (uint8_t)((promedio * 100UL + (LDR_ADC_MAXIMO / 2UL)) / LDR_ADC_MAXIMO);
    medicion->valida = 1U;
    medicion->ultimo_error_hal = 0UL;
    return HAL_OK;

error_adc:
    medicion->valida = 0U;
    medicion->ultimo_error_hal = 1UL;
    return HAL_ERROR;
}
