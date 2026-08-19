#ifndef RAUDY_H
#define RAUDY_H

#include <stdint.h>

/*======================
    PUERTOS DISPONIBLES
=======================*/
typedef enum
{
    RAUDY_GPIOA = 0,
    RAUDY_GPIOB,
    RAUDY_GPIOC,
    RAUDY_GPIOD,
    RAUDY_GPIOE,
    RAUDY_GPIOF
} RAUDY_PUERTO;

/*======================
      MODOS
=======================*/
typedef enum
{
    RAUDY_ENTRADA = 0,
    RAUDY_SALIDA

} RAUDY_MODO;

/*======================
    NIVELES LOGICOS
=======================*/
typedef enum
{
    RAUDY_BAJO = 0,
    RAUDY_ALTO

} RAUDY_ESTADO;

typedef enum
{
    RAUDY_SIN_PULL = 0,
    RAUDY_PULL_UP,
    RAUDY_PULL_DOWN
} RAUDY_PULL;
/*======================
      FUNCIONES
=======================*/
void Raudy_ConfigurarPull(RAUDY_PUERTO puerto,
                          uint8_t pin,
                          RAUDY_PULL pull);

void Raudy_ActivarPuerto(RAUDY_PUERTO puerto);

void Raudy_ConfigurarPin(RAUDY_PUERTO puerto,
                         uint8_t pin,
                         RAUDY_MODO modo);

void Raudy_EscribirPin(RAUDY_PUERTO puerto,
                       uint8_t pin,
                       RAUDY_ESTADO estado);

uint8_t Raudy_LeerPin(RAUDY_PUERTO puerto,
                      uint8_t pin);

void Raudy_AlternarPin(RAUDY_PUERTO puerto,
                       uint8_t pin);

void Raudy_EscribirBits(RAUDY_PUERTO puerto,
                        uint32_t mascara,
                        uint32_t valor);

uint32_t Raudy_LeerBits(RAUDY_PUERTO puerto,
                        uint32_t mascara);

#endif
