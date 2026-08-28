#ifndef SICAVI_CONFIG_H_
#define SICAVI_CONFIG_H_

#include <stdint.h>

#define SICAVI_FIRMWARE_VERSION         "1.3.0"

/* Pines de la NUCLEO-G071RB. Los valores son indices, no mascaras HAL. */
#define SICAVI_PIN_LDR_PA0              0U
#define SICAVI_PIN_LED_ACEPTADO_PA5     5U
#define SICAVI_PIN_LED_RECHAZADO_PA6    6U
#define SICAVI_PIN_LED_ESTADO_PA7       7U
#define SICAVI_PIN_MOTOR_PC7_D9         7U
#define SICAVI_PIN_EXPULSOR_PB1         1U
#define SICAVI_PIN_SENSOR_PA9_D8        9U
#define SICAVI_PIN_STOP_PC1             1U
#define SICAVI_PIN_START_PC13           13U

/*
 * El modulo infrarrojo del montaje mantiene OUT alto sin caja y lo lleva a
 * bajo al detectar. D8 usa por tanto flanco descendente y pull-up interno.
 */
#define SICAVI_SENSOR_ACTIVO_ALTO       0U
#define SICAVI_START_ACTIVO_BAJO        1U
#define SICAVI_STOP_ACTIVO_BAJO         1U
#define SICAVI_DRIVER_MOTOR_ACTIVO_ALTO 1U
#define SICAVI_RELE_EXPULSOR_ACTIVO_ALTO 1U

/* Ajustes iniciales de operacion. */
#define SICAVI_BLOQUEO_POR_LUZ_HABILITADO 1U
#define SICAVI_LUZ_MINIMA_PORCENTAJE    25U
#define SICAVI_LUZ_MAXIMA_PORCENTAJE    90U
#define SICAVI_AVANCE_TRAS_SENSOR_MS    400UL
#define SICAVI_ESTABILIZACION_IMAGEN_MS 300UL
#define SICAVI_TIMEOUT_VISION_MS        8000UL
#define SICAVI_TIMEOUT_COMUNICACION_MS  3500UL
#define SICAVI_TIEMPO_PASO_MS           1000UL
#define SICAVI_TIEMPO_EXPULSOR_MS       500UL
#define SICAVI_TELEMETRIA_MS            1000UL
#define SICAVI_MUESTREO_LDR_MS          200UL
#define SICAVI_ANTIRREBOTE_MS           30UL

#define SICAVI_TICK_MS                  10UL
#define SICAVI_MS_A_TICKS(ms)           ((uint32_t)((ms) / SICAVI_TICK_MS))

#endif /* SICAVI_CONFIG_H_ */
