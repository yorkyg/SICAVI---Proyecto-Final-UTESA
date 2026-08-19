/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * @file           : main.c
  * @brief          : Main program body
  ******************************************************************************
  * @attention
  *
  * Copyright (c) 2026 STMicroelectronics.
  * All rights reserved.
  *
  * This software is licensed under terms that can be found in the LICENSE file
  * in the root directory of this software component.
  * If no LICENSE file comes with this software, it is provided AS-IS.
  *
  ******************************************************************************
  */
/* USER CODE END Header */
/* Includes ------------------------------------------------------------------*/
#include "main.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */
#include "Raudy.h"
#include <stdio.h>
#include <string.h>

/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */
typedef enum
{
    PARADO = 0,
    TRANSPORTANDO,
    ESPERANDO_RESULTADO,
    RECHAZANDO,
    LIBERANDO_SENSOR,
    FALLA
} EstadoSICAVI;

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */
#define SENSOR_PIN       0U    /* PC0 */
#define STOP_PIN         1U    /* PC1 */
#define START_PIN        13U   /* PC13 */

#define MOTOR_PIN        0U    /* PB0 */
#define EXPULSOR_PIN     1U    /* PB1 */

#define LED_RUN_PIN      5U    /* PA5 */
#define LED_STOP_PIN     6U    /* PA6 */
#define LED_ALARMA_PIN   7U    /* PA7 */

#define SENSOR_ACTIVO    0U    /* Sensor activo en bajo */
#define BOTON_ACTIVO     0U    /* Pull-Up: botón presionado = 0 */

#define LUZ_MIN_ADC      800U  /* Calibrar después de probar el LDR */
#define TIMEOUT_UART_MS  5000UL
#define TIEMPO_EXPULSOR_MS 700UL
#define TIEMPO_LIBERAR_SENSOR_MS 3000UL

/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/
ADC_HandleTypeDef hadc1;

UART_HandleTypeDef huart2;

/* USER CODE BEGIN PV */
EstadoSICAVI estado = PARADO;

uint32_t idCaja = 0;
uint32_t inicioEstado = 0;
uint32_t ultimoSegundo = 0;
uint32_t ultimoReporte = 0;

uint32_t segundosSistema = 0;
uint32_t segundosMotor = 0;
uint32_t ciclos = 0;

uint16_t luzADC = 0;

volatile uint8_t rxByte;
volatile uint8_t rxIndice = 0;
volatile uint8_t tramaLista = 0;

char rxLinea[80];
char comando[80];

/* USER CODE END PV */

/* Private function prototypes -----------------------------------------------*/
void SystemClock_Config(void);
static void MX_GPIO_Init(void);
static void MX_USART2_UART_Init(void);
static void MX_ADC1_Init(void);
/* USER CODE BEGIN PFP */

/* USER CODE END PFP */

/* Private user code ---------------------------------------------------------*/
/* USER CODE BEGIN 0 */
void EnviarUART(const char *texto)
{
    HAL_UART_Transmit(&huart2, (uint8_t *)texto, strlen(texto), 100);
}

uint8_t SensorActivo(void)
{
    return Raudy_LeerPin(RAUDY_GPIOC, SENSOR_PIN) == SENSOR_ACTIVO;
}

uint8_t StartActivo(void)
{
    return Raudy_LeerPin(RAUDY_GPIOC, START_PIN) == BOTON_ACTIVO;
}

uint8_t StopActivo(void)
{
    return Raudy_LeerPin(RAUDY_GPIOC, STOP_PIN) == BOTON_ACTIVO;
}

void Motor(uint8_t encender)
{
    Raudy_EscribirPin(RAUDY_GPIOB, MOTOR_PIN,
                      encender ? RAUDY_ALTO : RAUDY_BAJO);
}

void Expulsor(uint8_t encender)
{
    Raudy_EscribirPin(RAUDY_GPIOB, EXPULSOR_PIN,
                      encender ? RAUDY_ALTO : RAUDY_BAJO);
}

void LEDs(uint8_t run, uint8_t stop, uint8_t alarma)
{
    Raudy_EscribirPin(RAUDY_GPIOA, LED_RUN_PIN,
                      run ? RAUDY_ALTO : RAUDY_BAJO);

    Raudy_EscribirPin(RAUDY_GPIOA, LED_STOP_PIN,
                      stop ? RAUDY_ALTO : RAUDY_BAJO);

    Raudy_EscribirPin(RAUDY_GPIOA, LED_ALARMA_PIN,
                      alarma ? RAUDY_ALTO : RAUDY_BAJO);
}

void SalidasSeguras(void)
{
    Motor(0);
    Expulsor(0);
}

uint16_t LeerLDR(void)
{
    HAL_ADC_Start(&hadc1);
    HAL_ADC_PollForConversion(&hadc1, 10);

    luzADC = (uint16_t)HAL_ADC_GetValue(&hadc1);

    HAL_ADC_Stop(&hadc1);

    return luzADC;
}

uint8_t LuzValida(void)
{
    return LeerLDR() >= LUZ_MIN_ADC;
}

void EnviarTelemetria(void)
{
    char mensaje[120];

    sprintf(mensaje,
            "TEL=HOURMETER;SYSTEM_S=%lu;MOTOR_S=%lu;CYCLES=%lu;LIGHT=%u\r\n",
            segundosSistema, segundosMotor, ciclos, luzADC);

    EnviarUART(mensaje);
}

void ActivarFalla(const char *motivo)
{
    SalidasSeguras();
    LEDs(0, 0, 1);
    estado = FALLA;

    EnviarUART(motivo);
    EnviarTelemetria();
}

void DetenerSistema(const char *motivo)
{
    SalidasSeguras();
    LEDs(0, 1, 0);
    estado = PARADO;

    EnviarUART(motivo);
    EnviarTelemetria();
}

void ProcesarComando(void)
{
    unsigned long idRecibido;
    char resultado[16];
    char mensaje[80];

    if (tramaLista == 0)
        return;

    strcpy(comando, rxLinea);
    tramaLista = 0;

    if (strcmp(comando, "CMD=START") == 0)
    {
        if (estado == PARADO && LuzValida())
        {
            estado = TRANSPORTANDO;
            Motor(1);
            LEDs(1, 0, 0);
            EnviarUART("EVT=RUN\r\n");
        }

        return;
    }

    if (strcmp(comando, "CMD=STOP") == 0)
    {
        DetenerSistema("EVT=STOPPED;REASON=PC\r\n");
        return;
    }

    if (sscanf(comando, "CMD=%15[^;];ID=%lu",
               resultado, &idRecibido) != 2)
    {
        EnviarUART("ERR=BAD_COMMAND\r\n");
        return;
    }

    if (estado != ESPERANDO_RESULTADO || idRecibido != idCaja)
    {
        EnviarUART("ERR=UNEXPECTED_ID\r\n");
        return;
    }

    if (strcmp(resultado, "GOOD") == 0 ||
        strcmp(resultado, "FORMA") == 0)
    {
        sprintf(mensaje, "EVT=FINISHED;ID=%04lu;RESULT=%s\r\n",
                idCaja, resultado);

        EnviarUART(mensaje);

        ciclos++;
        Motor(1);
        inicioEstado = HAL_GetTick();
        estado = LIBERANDO_SENSOR;
    }
    else if (strcmp(resultado, "REJECT") == 0)
    {
        Expulsor(1);
        inicioEstado = HAL_GetTick();
        estado = RECHAZANDO;
    }
    else
    {
        ActivarFalla("EVT=FAULT;REASON=UNKNOWN_RESULT\r\n");
    }
}

/* USER CODE END 0 */

/**
  * @brief  The application entry point.
  * @retval int
  */
int main(void)
{

  /* USER CODE BEGIN 1 */

  /* USER CODE END 1 */

  /* MCU Configuration--------------------------------------------------------*/

  /* Reset of all peripherals, Initializes the Flash interface and the Systick. */
  HAL_Init();

  /* USER CODE BEGIN Init */

  /* USER CODE END Init */

  /* Configure the system clock */
  SystemClock_Config();

  /* USER CODE BEGIN SysInit */

  /* USER CODE END SysInit */

  /* Initialize all configured peripherals */
  MX_GPIO_Init();
  MX_USART2_UART_Init();
  MX_ADC1_Init();
  /* USER CODE BEGIN 2 */
  HAL_ADCEx_Calibration_Start(&hadc1);

  /* Configuración con la librería Raudy */
  Raudy_ActivarPuerto(RAUDY_GPIOA);
  Raudy_ActivarPuerto(RAUDY_GPIOB);
  Raudy_ActivarPuerto(RAUDY_GPIOC);

  Raudy_ConfigurarPin(RAUDY_GPIOC, SENSOR_PIN, RAUDY_ENTRADA);
  Raudy_ConfigurarPin(RAUDY_GPIOC, STOP_PIN, RAUDY_ENTRADA);
  Raudy_ConfigurarPin(RAUDY_GPIOC, START_PIN, RAUDY_ENTRADA);

  Raudy_ConfigurarPull(RAUDY_GPIOC, SENSOR_PIN, RAUDY_PULL_UP);
  Raudy_ConfigurarPull(RAUDY_GPIOC, STOP_PIN, RAUDY_PULL_UP);
  Raudy_ConfigurarPull(RAUDY_GPIOC, START_PIN, RAUDY_PULL_UP);

  Raudy_ConfigurarPin(RAUDY_GPIOB, MOTOR_PIN, RAUDY_SALIDA);
  Raudy_ConfigurarPin(RAUDY_GPIOB, EXPULSOR_PIN, RAUDY_SALIDA);

  Raudy_ConfigurarPin(RAUDY_GPIOA, LED_RUN_PIN, RAUDY_SALIDA);
  Raudy_ConfigurarPin(RAUDY_GPIOA, LED_STOP_PIN, RAUDY_SALIDA);
  Raudy_ConfigurarPin(RAUDY_GPIOA, LED_ALARMA_PIN, RAUDY_SALIDA);

  SalidasSeguras();
  LEDs(0, 1, 0);

  HAL_UART_Receive_IT(&huart2, (uint8_t *)&rxByte, 1);

  ultimoSegundo = HAL_GetTick();
  ultimoReporte = HAL_GetTick();

  EnviarUART("EVT=READY\r\n");

  /* USER CODE END 2 */

  /* Infinite loop */
  /* USER CODE BEGIN WHILE */
  while (1)
  {
    /* USER CODE END WHILE */

    /* USER CODE BEGIN 3 */
	  uint32_t ahora = HAL_GetTick();

	  ProcesarComando();

	  /* Horómetro */
	  if (ahora - ultimoSegundo >= 1000UL)
	  {
	      ultimoSegundo += 1000UL;

	      segundosSistema++;

	      if (estado == TRANSPORTANDO || estado == LIBERANDO_SENSOR)
	      {
	          segundosMotor++;
	      }
	  }

	  /* Telemetría cada minuto */
	  if (ahora - ultimoReporte >= 60000UL)
	  {
	      ultimoReporte += 60000UL;
	      EnviarTelemetria();
	  }

	  /* STOP físico: siempre tiene prioridad */
	  if (StopActivo() && estado != PARADO)
	  {
	      DetenerSistema("EVT=STOPPED;REASON=LOCAL_STOP\r\n");
	  }

	  switch (estado)
	  {
	      case PARADO:

	          if (StartActivo())
	          {
	              if (LuzValida())
	              {
	                  Motor(1);
	                  LEDs(1, 0, 0);
	                  estado = TRANSPORTANDO;

	                  EnviarUART("EVT=RUN\r\n");
	              }
	              else
	              {
	                  ActivarFalla("EVT=FAULT;REASON=LIGHT\r\n");
	              }
	          }

	          break;

	      case TRANSPORTANDO:

	          if (!LuzValida())
	          {
	              ActivarFalla("EVT=FAULT;REASON=LIGHT\r\n");
	              break;
	          }

	          if (SensorActivo())
	          {
	              char mensaje[80];

	              Motor(0);

	              idCaja++;
	              inicioEstado = ahora;
	              estado = ESPERANDO_RESULTADO;

	              sprintf(mensaje, "EVT=DETECTED;ID=%04lu;LIGHT=%u\r\n",
	                      idCaja, luzADC);

	              EnviarUART(mensaje);
	          }

	          break;

	      case ESPERANDO_RESULTADO:

	          if (ahora - inicioEstado >= TIMEOUT_UART_MS)
	          {
	              ActivarFalla("EVT=FAULT;REASON=UART_TIMEOUT\r\n");
	          }

	          break;

	      case RECHAZANDO:

	          if (ahora - inicioEstado >= TIEMPO_EXPULSOR_MS)
	          {
	              char mensaje[80];

	              Expulsor(0);

	              sprintf(mensaje,
	                      "EVT=FINISHED;ID=%04lu;RESULT=REJECT\r\n",
	                      idCaja);

	              EnviarUART(mensaje);

	              ciclos++;
	              Motor(1);

	              inicioEstado = ahora;
	              estado = LIBERANDO_SENSOR;
	          }

	          break;

	      case LIBERANDO_SENSOR:

	          if (!SensorActivo())
	          {
	              estado = TRANSPORTANDO;
	          }
	          else if (ahora - inicioEstado >= TIEMPO_LIBERAR_SENSOR_MS)
	          {
	              ActivarFalla("EVT=FAULT;REASON=SENSOR_BLOCKED\r\n");
	          }

	          break;

	      case FALLA:

	          if (StartActivo() && LuzValida())
	          {
	              Motor(1);
	              LEDs(1, 0, 0);
	              estado = TRANSPORTANDO;

	              EnviarUART("EVT=RUN\r\n");
	          }

	          break;
	  }

  }
  /* USER CODE END 3 */
}

/**
  * @brief System Clock Configuration
  * @retval None
  */
void SystemClock_Config(void)
{
  RCC_OscInitTypeDef RCC_OscInitStruct = {0};
  RCC_ClkInitTypeDef RCC_ClkInitStruct = {0};

  /** Configure the main internal regulator output voltage
  */
  HAL_PWREx_ControlVoltageScaling(PWR_REGULATOR_VOLTAGE_SCALE1);

  /** Initializes the RCC Oscillators according to the specified parameters
  * in the RCC_OscInitTypeDef structure.
  */
  RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSI;
  RCC_OscInitStruct.HSIState = RCC_HSI_ON;
  RCC_OscInitStruct.HSIDiv = RCC_HSI_DIV1;
  RCC_OscInitStruct.HSICalibrationValue = RCC_HSICALIBRATION_DEFAULT;
  RCC_OscInitStruct.PLL.PLLState = RCC_PLL_NONE;
  if (HAL_RCC_OscConfig(&RCC_OscInitStruct) != HAL_OK)
  {
    Error_Handler();
  }

  /** Initializes the CPU, AHB and APB buses clocks
  */
  RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK|RCC_CLOCKTYPE_SYSCLK
                              |RCC_CLOCKTYPE_PCLK1;
  RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_HSI;
  RCC_ClkInitStruct.AHBCLKDivider = RCC_SYSCLK_DIV1;
  RCC_ClkInitStruct.APB1CLKDivider = RCC_HCLK_DIV1;

  if (HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_0) != HAL_OK)
  {
    Error_Handler();
  }
}

/**
  * @brief ADC1 Initialization Function
  * @param None
  * @retval None
  */
static void MX_ADC1_Init(void)
{

  /* USER CODE BEGIN ADC1_Init 0 */

  /* USER CODE END ADC1_Init 0 */

  ADC_ChannelConfTypeDef sConfig = {0};

  /* USER CODE BEGIN ADC1_Init 1 */

  /* USER CODE END ADC1_Init 1 */

  /** Configure the global features of the ADC (Clock, Resolution, Data Alignment and number of conversion)
  */
  hadc1.Instance = ADC1;
  hadc1.Init.ClockPrescaler = ADC_CLOCK_SYNC_PCLK_DIV2;
  hadc1.Init.Resolution = ADC_RESOLUTION_12B;
  hadc1.Init.DataAlign = ADC_DATAALIGN_RIGHT;
  hadc1.Init.ScanConvMode = ADC_SCAN_DISABLE;
  hadc1.Init.EOCSelection = ADC_EOC_SINGLE_CONV;
  hadc1.Init.LowPowerAutoWait = DISABLE;
  hadc1.Init.LowPowerAutoPowerOff = DISABLE;
  hadc1.Init.ContinuousConvMode = DISABLE;
  hadc1.Init.NbrOfConversion = 1;
  hadc1.Init.DiscontinuousConvMode = DISABLE;
  hadc1.Init.ExternalTrigConv = ADC_SOFTWARE_START;
  hadc1.Init.ExternalTrigConvEdge = ADC_EXTERNALTRIGCONVEDGE_NONE;
  hadc1.Init.DMAContinuousRequests = DISABLE;
  hadc1.Init.Overrun = ADC_OVR_DATA_PRESERVED;
  hadc1.Init.SamplingTimeCommon1 = ADC_SAMPLETIME_1CYCLE_5;
  hadc1.Init.SamplingTimeCommon2 = ADC_SAMPLETIME_1CYCLE_5;
  hadc1.Init.OversamplingMode = DISABLE;
  hadc1.Init.TriggerFrequencyMode = ADC_TRIGGER_FREQ_HIGH;
  if (HAL_ADC_Init(&hadc1) != HAL_OK)
  {
    Error_Handler();
  }

  /** Configure Regular Channel
  */
  sConfig.Channel = ADC_CHANNEL_0;
  sConfig.Rank = ADC_REGULAR_RANK_1;
  sConfig.SamplingTime = ADC_SAMPLINGTIME_COMMON_1;
  if (HAL_ADC_ConfigChannel(&hadc1, &sConfig) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN ADC1_Init 2 */

  /* USER CODE END ADC1_Init 2 */

}

/**
  * @brief USART2 Initialization Function
  * @param None
  * @retval None
  */
static void MX_USART2_UART_Init(void)
{

  /* USER CODE BEGIN USART2_Init 0 */

  /* USER CODE END USART2_Init 0 */

  /* USER CODE BEGIN USART2_Init 1 */

  /* USER CODE END USART2_Init 1 */
  huart2.Instance = USART2;
  huart2.Init.BaudRate = 115200;
  huart2.Init.WordLength = UART_WORDLENGTH_8B;
  huart2.Init.StopBits = UART_STOPBITS_1;
  huart2.Init.Parity = UART_PARITY_NONE;
  huart2.Init.Mode = UART_MODE_TX_RX;
  huart2.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart2.Init.OverSampling = UART_OVERSAMPLING_16;
  huart2.Init.OneBitSampling = UART_ONE_BIT_SAMPLE_DISABLE;
  huart2.Init.ClockPrescaler = UART_PRESCALER_DIV1;
  huart2.AdvancedInit.AdvFeatureInit = UART_ADVFEATURE_NO_INIT;
  if (HAL_UART_Init(&huart2) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_UARTEx_SetTxFifoThreshold(&huart2, UART_TXFIFO_THRESHOLD_1_8) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_UARTEx_SetRxFifoThreshold(&huart2, UART_RXFIFO_THRESHOLD_1_8) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_UARTEx_DisableFifoMode(&huart2) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN USART2_Init 2 */

  /* USER CODE END USART2_Init 2 */

}

/**
  * @brief GPIO Initialization Function
  * @param None
  * @retval None
  */
static void MX_GPIO_Init(void)
{
  GPIO_InitTypeDef GPIO_InitStruct = {0};
  /* USER CODE BEGIN MX_GPIO_Init_1 */

  /* USER CODE END MX_GPIO_Init_1 */

  /* GPIO Ports Clock Enable */
  __HAL_RCC_GPIOC_CLK_ENABLE();
  __HAL_RCC_GPIOF_CLK_ENABLE();
  __HAL_RCC_GPIOA_CLK_ENABLE();
  __HAL_RCC_GPIOB_CLK_ENABLE();

  /*Configure GPIO pin Output Level */
  HAL_GPIO_WritePin(GPIOA, LED_GREEN_Pin|GPIO_PIN_6|GPIO_PIN_7, GPIO_PIN_RESET);

  /*Configure GPIO pin Output Level */
  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_0|GPIO_PIN_1, GPIO_PIN_RESET);

  /*Configure GPIO pins : PC0 PC1 */
  GPIO_InitStruct.Pin = GPIO_PIN_0|GPIO_PIN_1;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  HAL_GPIO_Init(GPIOC, &GPIO_InitStruct);

  /*Configure GPIO pin : LED_GREEN_Pin */
  GPIO_InitStruct.Pin = LED_GREEN_Pin;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_HIGH;
  HAL_GPIO_Init(LED_GREEN_GPIO_Port, &GPIO_InitStruct);

  /*Configure GPIO pins : PA6 PA7 */
  GPIO_InitStruct.Pin = GPIO_PIN_6|GPIO_PIN_7;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pins : PB0 PB1 */
  GPIO_InitStruct.Pin = GPIO_PIN_0|GPIO_PIN_1;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

  /* USER CODE BEGIN MX_GPIO_Init_2 */

  /* USER CODE END MX_GPIO_Init_2 */
}

/* USER CODE BEGIN 4 */
void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart)
{
    if (huart->Instance == USART2)
    {
        if (rxByte == '\n' || rxByte == '\r')
        {
            if (rxIndice > 0U)
            {
                rxLinea[rxIndice] = '\0';
                rxIndice = 0U;
                tramaLista = 1U;
            }
        }
        else if (rxIndice < sizeof(rxLinea) - 1U && tramaLista == 0U)
        {
            rxLinea[rxIndice] = (char)rxByte;
            rxIndice++;
        }

        HAL_UART_Receive_IT(&huart2, (uint8_t *)&rxByte, 1);
    }
}

/* USER CODE END 4 */

/**
  * @brief  This function is executed in case of error occurrence.
  * @retval None
  */
void Error_Handler(void)
{
  /* USER CODE BEGIN Error_Handler_Debug */
  /* User can add his own implementation to report the HAL error return state */
  __disable_irq();
  while (1)
  {
  }
  /* USER CODE END Error_Handler_Debug */
}
#ifdef USE_FULL_ASSERT
/**
  * @brief  Reports the name of the source file and the source line number
  *         where the assert_param error has occurred.
  * @param  file: pointer to the source file name
  * @param  line: assert_param error line source number
  * @retval None
  */
void assert_failed(uint8_t *file, uint32_t line)
{
  /* USER CODE BEGIN 6 */
  /* User can add his own implementation to report the file name and line number,
     ex: printf("Wrong parameters value: file %s on line %d\r\n", file, line) */
  /* USER CODE END 6 */
}
#endif /* USE_FULL_ASSERT */
