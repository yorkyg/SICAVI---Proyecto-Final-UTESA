#include "Raudy.h"

/*==========================
      DIRECCIONES BASE
===========================*/

#define RCC_BASE        0x40021000UL

#define GPIOA_BASE      0x50000000UL
#define GPIOB_BASE      0x50000400UL
#define GPIOC_BASE      0x50000800UL
#define GPIOD_BASE      0x50000C00UL
#define GPIOE_BASE      0x50001000UL
#define GPIOF_BASE      0x50001400UL

/*==========================
        ESTRUCTURAS
===========================*/

typedef struct
{
    volatile uint32_t MODER;
    volatile uint32_t OTYPER;
    volatile uint32_t OSPEEDR;
    volatile uint32_t PUPDR;
    volatile uint32_t IDR;
    volatile uint32_t ODR;
    volatile uint32_t BSRR;

}GPIO_RAUDY;

typedef struct
{
    volatile uint32_t CR;
    volatile uint32_t ICSCR;
    volatile uint32_t CFGR;
    volatile uint32_t PLLCFGR;

    volatile uint32_t RESERVED0[2];

    volatile uint32_t CIER;
    volatile uint32_t CIFR;
    volatile uint32_t CICR;
    volatile uint32_t IOPRSTR;
    volatile uint32_t AHBRSTR;
    volatile uint32_t APBRSTR1;
    volatile uint32_t APBRSTR2;

    volatile uint32_t IOPENR;

} RCC_RAUDY;


/*==========================
        PUNTEROS
===========================*/

#define RCC ((RCC_RAUDY*)RCC_BASE)

static GPIO_RAUDY *const gpio[]=
{
    (GPIO_RAUDY*)GPIOA_BASE,
    (GPIO_RAUDY*)GPIOB_BASE,
    (GPIO_RAUDY*)GPIOC_BASE,
    (GPIO_RAUDY*)GPIOD_BASE,
	(GPIO_RAUDY*)GPIOE_BASE,
    (GPIO_RAUDY*)GPIOF_BASE
};


/*==========================
      ACTIVAR PUERTO
===========================*/

void Raudy_ActivarPuerto(RAUDY_PUERTO puerto)
{
    RCC->IOPENR |= (1U<<puerto);
}


/*==========================
    CONFIGURAR PIN
===========================*/

void Raudy_ConfigurarPin(RAUDY_PUERTO puerto,
                         uint8_t pin,
                         RAUDY_MODO modo)
{

    gpio[puerto]->MODER &= ~(3U<<(pin*2));

    if(modo==RAUDY_SALIDA)
    {
        gpio[puerto]->MODER |= (1U<<(pin*2));
    }

}

void Raudy_ConfigurarPull(RAUDY_PUERTO puerto,
                          uint8_t pin,
                          RAUDY_PULL pull)
{
    gpio[puerto]->PUPDR &= ~(3U << (pin * 2U));
    gpio[puerto]->PUPDR |= ((uint32_t)pull << (pin * 2U));
}

/*==========================
      ESCRIBIR PIN
===========================*/

void Raudy_EscribirPin(RAUDY_PUERTO puerto,
                       uint8_t pin,
                       RAUDY_ESTADO estado)
{

    if(estado==RAUDY_ALTO)
    {
        gpio[puerto]->BSRR=(1U<<pin);
    }
    else
    {
        gpio[puerto]->BSRR=(1U<<(pin+16));
    }

}


/*==========================
        LEER PIN
===========================*/

uint8_t Raudy_LeerPin(RAUDY_PUERTO puerto,
                      uint8_t pin)
{

    return (gpio[puerto]->IDR>>pin)&1U;

}


/*==========================
      ALTERNAR PIN
===========================*/

void Raudy_AlternarPin(RAUDY_PUERTO puerto,
                       uint8_t pin)
{

    gpio[puerto]->ODR ^= (1U<<pin);

}


/*==========================
      ESCRIBIR BITS
===========================*/

void Raudy_EscribirBits(RAUDY_PUERTO puerto,
                        uint32_t mascara,
                        uint32_t valor)
{

    gpio[puerto]->ODR &= ~mascara;

    gpio[puerto]->ODR |= (valor & mascara);

}


/*==========================
      LEER BITS
===========================*/

uint32_t Raudy_LeerBits(RAUDY_PUERTO puerto,
                        uint32_t mascara)
{

    return gpio[puerto]->IDR & mascara;

}
