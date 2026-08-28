#ifndef LDR_H_
#define LDR_H_

#include "stm32g0xx_hal.h"
#include <stdint.h>

typedef struct
{
    uint16_t adc;
    uint16_t milivoltios;
    uint8_t porcentaje;
    uint8_t valida;
    uint32_t ultimo_error_hal;
} MedicionLdr;

HAL_StatusTypeDef ldr_inicializar(void);
HAL_StatusTypeDef ldr_actualizar(MedicionLdr *medicion);

#endif /* LDR_H_ */
