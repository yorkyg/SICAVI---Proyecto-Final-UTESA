#include "horometro.h"

void horometro_inicializar(HorometroSicavi *horometro)
{
    if (horometro == 0) return;
    horometro->segundos_trabajo = 0UL;
    horometro->ciclos_expulsor = 0UL;
}

void horometro_tick_1s(HorometroSicavi *horometro, uint8_t trabajo_activo)
{
    if (horometro == 0) return;
    if (trabajo_activo != 0U) horometro->segundos_trabajo++;
}

void horometro_registrar_ciclo_expulsor(HorometroSicavi *horometro)
{
    if (horometro == 0) return;
    horometro->ciclos_expulsor++;
}
