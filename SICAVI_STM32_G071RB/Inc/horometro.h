#ifndef HOROMETRO_H_
#define HOROMETRO_H_

#include <stdint.h>

typedef struct
{
    uint32_t segundos_trabajo;
    uint32_t ciclos_expulsor;
} HorometroSicavi;

void horometro_inicializar(HorometroSicavi *horometro);
void horometro_tick_1s(HorometroSicavi *horometro, uint8_t trabajo_activo);
void horometro_registrar_ciclo_expulsor(HorometroSicavi *horometro);

#endif /* HOROMETRO_H_ */
