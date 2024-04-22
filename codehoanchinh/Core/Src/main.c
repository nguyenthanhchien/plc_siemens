/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * @file           : main.c
  * @brief          : Main program body
  ******************************************************************************
  * @attention
  *
  * Copyright (c) 2023 STMicroelectronics.
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
#include <stdlib.h>
#include <stdio.h> // for sprintf and sscanf
#include <string.h>

/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */

/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/
TIM_HandleTypeDef htim1;
TIM_HandleTypeDef htim2;
TIM_HandleTypeDef htim3;
TIM_HandleTypeDef htim4;

UART_HandleTypeDef huart3;
DMA_HandleTypeDef hdma_usart3_rx;

/* USER CODE BEGIN PV */
#define pick HAL_GPIO_WritePin(GPIOB, GPIO_PIN_12, GPIO_PIN_SET)
#define drop HAL_GPIO_WritePin(GPIOB, GPIO_PIN_12, GPIO_PIN_RESET)
/* USER CODE END PV */

/* Private function prototypes -----------------------------------------------*/
void SystemClock_Config(void);
static void MX_GPIO_Init(void);
static void MX_DMA_Init(void);
static void MX_TIM1_Init(void);
static void MX_TIM2_Init(void);
static void MX_TIM3_Init(void);
static void MX_TIM4_Init(void);
static void MX_USART3_UART_Init(void);
/* USER CODE BEGIN PFP */
uint8_t buff[10];uint16_t size ;
#define RX_SIZE 10
uint8_t rxBuffer[RX_SIZE] = {0};
uint8_t uart_buff[10];
uint8_t count,READ,len;
char char_buff[10];
int a,b,c;
float value;
float vitridat,vitridat2,vitridat3;
float vitridat_last;
uint8_t flag;
uint8_t sensorValue1,sensorValue2,sensorValue3;
int flag1,flag2,flag3;
//const int phase_a = GPIO_PIN_0;  // Ch?n GPIO_PIN tuong ?ng v?i phase_a
//const int phase_b = GPIO_PIN_1;  // Ch?n GPIO_PIN tuong ?ng v?i phase_b
const int encoder_pulse = 1793;  // S? xung m?i vòng (thay d?i theo ph?n c?ng)

float T,pulseLast;
volatile float pulse=0;
float E,E1,E2;
float vitri,vitri_last,vitridat_last;
float alpha, beta, gamma, Kp, Kd, Ki;
float Output,LastOutput;

int dao;
	
float pulseLast2;
volatile float pulse2=0;
float Ea,E1a,E2a;
float vitri2,vitri_last2,vitridat_last2;
float alpha2, beta2, gamma2, Kp2, Kd2, Ki2;
float Output2,LastOutput2;

float pulseLast3;
volatile float pulse3=0;
float Eb,E1b,E2b;
float vitri3,vitri_last3,vitridat_last3;
float alpha3, beta3, gamma3, Kp3, Kd3, Ki3;
float Output3,LastOutput3;

void checkSetHomeDC1()
{
	if(sensorValue1==1)
	{
		if(flag1==0)
		{
			HAL_TIM_PWM_Stop(&htim1, TIM_CHANNEL_1);
		vitridat= 0;vitri=0;pulse=0;
		flag1=1;
	}
		}
		if(flag1==1)
		{
			HAL_TIM_PWM_Start(&htim1, TIM_CHANNEL_1);
			vitridat=140*40/16;
			if(vitri>vitridat-10 && vitri<vitridat+5)
				{
					pulse=0;
					vitri=0; vitridat=0;flag1=0;
				}
		}
}
		

	
void checkSetHomeDC2()
{
	if(sensorValue2==1)
	{
		if(flag2==0)	
		{
			HAL_TIM_PWM_Stop(&htim3, TIM_CHANNEL_1);
			vitridat2=0;vitri2=0;pulse2=0;
			flag2=1;
		}
	}
		if(flag2==1)
		{
			HAL_TIM_PWM_Start(&htim3, TIM_CHANNEL_1);
			vitridat2=180*60/16;
			if(vitri2>vitridat2-3 && vitri2<vitridat2+3)
				{
					pulse2=0;
					vitri2=0; vitridat2=0;flag2=0;
				}
		}
}
void checkSetHomeDC3()
{
	if(sensorValue3==1)
	{
		if(flag3==0)
		{
			HAL_TIM_PWM_Stop(&htim4, TIM_CHANNEL_1);
			vitridat3=0;vitri3=0;pulse3=0;
			flag3=1;
		}
	}
		if(flag3==1)
		{
			HAL_TIM_PWM_Start(&htim4, TIM_CHANNEL_1);
			vitridat3=-95*60/16;
			if(vitri3>vitridat3-10&& vitri3<vitridat3+5)
				{
					
					pulse3=0;
					vitri3=0; vitridat3=0;flag3=0;
				}
		}
}
void pick_drop()
{
	if(c==2)
	{
		if(dao%2==0)
		{
			drop;
		}
		else{
			pick;
		}
	}
}
void readSensor()
{
	sensorValue1 = HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_13);
	sensorValue2 = HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_8);
	sensorValue3 = HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_9);
	checkSetHomeDC1();
	checkSetHomeDC2();
	checkSetHomeDC3();
	READ = HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_12);
	//pick_drop();
}

void HAL_GPIO_EXTI_Callback(uint16_t GPIO_Pin)
{
    if (GPIO_Pin == GPIO_PIN_0) // GPIO_PIN_0 là nút nh?n k?t n?i d?n chân GPIOx_PIN_0
    {
         if (HAL_GPIO_ReadPin(GPIOA, GPIO_PIN_1) == GPIO_PIN_SET)
        {
            pulse--;
        }
        else
        {
            pulse++;
        }
    }
		else if(GPIO_Pin == GPIO_PIN_1)
		{
			if (HAL_GPIO_ReadPin(GPIOB, GPIO_PIN_2) == GPIO_PIN_SET)
        {
            pulse2++;
        }
        else
        {
            pulse2--;
        }
		}
		else if(GPIO_Pin == GPIO_PIN_3)
		{
			if (HAL_GPIO_ReadPin(GPIOA, GPIO_PIN_15) == GPIO_PIN_SET)
        {
					a++;
            pulse3--;
        }
        else
        {
					a--;
            pulse3++;
        }
		}
}


void PID() {
	  a++;
    vitri = pulse * 360.0 / encoder_pulse;
    E = vitridat - vitri;
    alpha = 2 * T * Kp + Ki * T * T + 2 * Kd;
    beta = T * T * Ki - 4 * Kd - 2 * T * Kp;
    gamma = 2 * Kd;
    Output = (alpha * E + beta * E1 + gamma * E2 + 2 * T * LastOutput) / (2 * T);
    LastOutput = Output;
    E2 = E1;
    E1 = E;

    if (Output > 254) {
        Output = 254;
			
    }
    if (Output < -254) {
        Output = -254;
			
    }
		
		
    if (Output > 0) {
        __HAL_TIM_SetCompare(&htim1,TIM_CHANNEL_1,Output/4);
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_9, 1);
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_10, 0);
    } else if (Output < 0) {
        __HAL_TIM_SetCompare(&htim1,TIM_CHANNEL_1,abs((int)(Output/4)));
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_9, 0);
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_10, 1);
    } else {
        __HAL_TIM_SetCompare(&htim1,TIM_CHANNEL_1,0);
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_9, 0);
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_10, 0);
    }
}

void PID2() {
		
    vitri2 = pulse2 * 360.0 / encoder_pulse;
    Ea = vitridat2 - vitri2;
    alpha2 = 2 * T * Kp2 + Ki2 * T * T + 2 * Kd2;
    beta2 = T * T * Ki2 - 4 * Kd2 - 2 * T * Kp2;
    gamma2 = 2 * Kd2;
    Output2 = (alpha2 * Ea + beta2 * E1a + gamma2 * E2a + 2 * T * LastOutput2) / (2 * T);
    LastOutput2 = Output2;
    E2a = E1a;
    E1a = Ea;

    if (Output2 > 254) {
        Output2 = 254;
			
    }
    if (Output2 < -254) {
        Output2 = -254;
			
    }
		/*test=(int)Output*100/255;*/
		
    if (Output2 > 0) {
        __HAL_TIM_SetCompare(&htim3,TIM_CHANNEL_1,(Output2)/3);
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_4, 1);
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_5, 0);
    } else if (Output2 < 0) {
        __HAL_TIM_SetCompare(&htim3,TIM_CHANNEL_1,abs((int)((Output2)/3)));
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_4, 0);
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_5, 1);
    } else {
        __HAL_TIM_SetCompare(&htim3,TIM_CHANNEL_1,0);
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_4, 0);
        HAL_GPIO_WritePin(GPIOA, GPIO_PIN_5, 0);
    }
}

void PID3() {
		//c++;
    vitri3 = pulse3 * 360.0 / encoder_pulse;
    Eb = vitridat3 - vitri3;
    alpha3 = 2 * T * Kp3 + Ki3 * T * T + 2 * Kd3;
    beta3 = T * T * Ki3 - 4 * Kd3 - 2 * T * Kp3;
    gamma3 = 2 * Kd3;
    Output3 = (alpha3 * Eb + beta3 * E1b + gamma3 * E2b + 2 * T * LastOutput3) / (2 * T);
    LastOutput3 = Output3;
    E2b = E1b;
    E1b = Eb;

    if (Output3 > 254) {
        Output3 = 254;
			
    }
    if (Output3 < -254) {
        Output3 = -254;
			
    }
		/*test=(int)Output*100/255;*/
		
    if (Output3 > 0) {
        __HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_1,(Output3)/3);
        HAL_GPIO_WritePin(GPIOB, GPIO_PIN_4, 1);
        HAL_GPIO_WritePin(GPIOB, GPIO_PIN_5, 0);
    } else if (Output3 < 0) {
        __HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_1,abs((int)((Output3)/3)));
        HAL_GPIO_WritePin(GPIOB, GPIO_PIN_4, 0);
        HAL_GPIO_WritePin(GPIOB, GPIO_PIN_5, 1);
    } else {
        __HAL_TIM_SetCompare(&htim4,TIM_CHANNEL_1,0);
        HAL_GPIO_WritePin(GPIOB, GPIO_PIN_4, 0);
        HAL_GPIO_WritePin(GPIOB, GPIO_PIN_5, 0);
    }
}

void HAL_TIM_PeriodElapsedCallback(TIM_HandleTypeDef *htim) {
	/*static uint16_t cnt =99;*/
  if (htim->Instance == htim2.Instance) {
			readSensor();
      PID();
			//PID2();
			//PID3();	  
	}
	
}


void HAL_UARTEx_RxEventCallback(UART_HandleTypeDef *huart, uint16_t Size)
{
	
	if(rxBuffer[0] == '\n')
	{
		flag=1;len=0;//HAL_GPIO_WritePin(GPIOB, GPIO_PIN_12, GPIO_PIN_SET);
	}
	else
	{
		uart_buff[len++] = rxBuffer[0];
	}
}
/* USER CODE END PFP */

/* Private user code ---------------------------------------------------------*/
/* USER CODE BEGIN 0 */

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
  MX_DMA_Init();
  MX_TIM1_Init();
  MX_TIM2_Init();
  MX_TIM3_Init();
  MX_TIM4_Init();
  MX_USART3_UART_Init();
  /* USER CODE BEGIN 2 */
	//HAL_UART_Receive_IT(&huart3, &data_rx,1);
	
	HAL_UARTEx_ReceiveToIdle_DMA(&huart3, rxBuffer, 1);
	__HAL_DMA_DISABLE_IT(&hdma_usart3_rx, DMA_IT_HT);
	HAL_TIM_PWM_Start(&htim1,TIM_CHANNEL_1);
	HAL_TIM_Base_Start_IT(&htim2);
	HAL_TIM_PWM_Start(&htim3,TIM_CHANNEL_1);
	HAL_TIM_PWM_Start(&htim4,TIM_CHANNEL_1);
	
	
	
	vitri=0;count=0;
	E=0;E1=0;E2=0;
	Output=0;LastOutput=0;
	 //Thong So PID
	T=0.01;// Thoi gian lay ma
	Kp =32.5;Kd=5.55;Ki=0.001;
	
	vitri2=0;Ea=0;E1a=0;E2a=0;Kp2 =60;Kd2=0.55;Ki2=0.001;Output2=0;LastOutput2=0;
	vitri3=0;Eb=0;E1b=0;E2b=0;Kp3 =13.9;Kd3=0.02;Ki3=0.02;Output3=0;LastOutput3=0;
	
	a=0;b=0;int d=0;
	vitridat=0;vitridat2=0;vitridat3=0;
	//vitridat_last =0;
	char* data;
  flag2=0;flag3=0;flag1=0;flag=0;  value=0; c =0;size=0;len=0;
  /* USER CODE END 2 */

  /* Infinite loop */
  /* USER CODE BEGIN WHILE */
  while (1)
  {
		HAL_Delay(1);
		if(flag==1)
		{
			
				for (int i = 0; i < 10; i++) {
        char_buff[i] = (char) uart_buff[i];
			}
			memset(uart_buff, 0, 10);
      
				char* token = strtok(char_buff, ",");
				if (token != NULL) {
						vitridat = strtof(token, NULL)*40/16;
						
						}

				// Move to the next token
				token = strtok(NULL, ",");

					
				// Convert the second token to float and store it in vitridat2
				if (token != NULL) {
						vitridat2 = strtof(token, NULL)*60/16;
						}

				// Move to the third token
				token = strtok(NULL, ",");
        if (token != NULL) {
						vitridat3 = strtof(token, NULL)*60/16;
						}
				// Convert the third token to float and store it in vitridat3
				
				HAL_TIM_PWM_Start(&htim1, TIM_CHANNEL_1);
				HAL_TIM_PWM_Start(&htim3, TIM_CHANNEL_1);
				HAL_TIM_PWM_Start(&htim4, TIM_CHANNEL_1);
				flag=0;
						}
    /* USER CODE END WHILE */

    /* USER CODE BEGIN 3 */
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

  /** Initializes the RCC Oscillators according to the specified parameters
  * in the RCC_OscInitTypeDef structure.
  */
  RCC_OscInitStruct.OscillatorType = RCC_OSCILLATORTYPE_HSE;
  RCC_OscInitStruct.HSEState = RCC_HSE_ON;
  RCC_OscInitStruct.HSEPredivValue = RCC_HSE_PREDIV_DIV1;
  RCC_OscInitStruct.HSIState = RCC_HSI_ON;
  RCC_OscInitStruct.PLL.PLLState = RCC_PLL_ON;
  RCC_OscInitStruct.PLL.PLLSource = RCC_PLLSOURCE_HSE;
  RCC_OscInitStruct.PLL.PLLMUL = RCC_PLL_MUL6;
  if (HAL_RCC_OscConfig(&RCC_OscInitStruct) != HAL_OK)
  {
    Error_Handler();
  }

  /** Initializes the CPU, AHB and APB buses clocks
  */
  RCC_ClkInitStruct.ClockType = RCC_CLOCKTYPE_HCLK|RCC_CLOCKTYPE_SYSCLK
                              |RCC_CLOCKTYPE_PCLK1|RCC_CLOCKTYPE_PCLK2;
  RCC_ClkInitStruct.SYSCLKSource = RCC_SYSCLKSOURCE_PLLCLK;
  RCC_ClkInitStruct.AHBCLKDivider = RCC_SYSCLK_DIV1;
  RCC_ClkInitStruct.APB1CLKDivider = RCC_HCLK_DIV2;
  RCC_ClkInitStruct.APB2CLKDivider = RCC_HCLK_DIV1;

  if (HAL_RCC_ClockConfig(&RCC_ClkInitStruct, FLASH_LATENCY_1) != HAL_OK)
  {
    Error_Handler();
  }
}

/**
  * @brief TIM1 Initialization Function
  * @param None
  * @retval None
  */
static void MX_TIM1_Init(void)
{

  /* USER CODE BEGIN TIM1_Init 0 */

  /* USER CODE END TIM1_Init 0 */

  TIM_ClockConfigTypeDef sClockSourceConfig = {0};
  TIM_MasterConfigTypeDef sMasterConfig = {0};
  TIM_OC_InitTypeDef sConfigOC = {0};
  TIM_BreakDeadTimeConfigTypeDef sBreakDeadTimeConfig = {0};

  /* USER CODE BEGIN TIM1_Init 1 */

  /* USER CODE END TIM1_Init 1 */
  htim1.Instance = TIM1;
  htim1.Init.Prescaler = 1881;
  htim1.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim1.Init.Period = 254;
  htim1.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim1.Init.RepetitionCounter = 0;
  htim1.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_Base_Init(&htim1) != HAL_OK)
  {
    Error_Handler();
  }
  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
  if (HAL_TIM_ConfigClockSource(&htim1, &sClockSourceConfig) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_TIM_PWM_Init(&htim1) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim1, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sConfigOC.OCMode = TIM_OCMODE_PWM1;
  sConfigOC.Pulse = 0;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_HIGH;
  sConfigOC.OCNPolarity = TIM_OCNPOLARITY_HIGH;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  sConfigOC.OCIdleState = TIM_OCIDLESTATE_RESET;
  sConfigOC.OCNIdleState = TIM_OCNIDLESTATE_RESET;
  if (HAL_TIM_PWM_ConfigChannel(&htim1, &sConfigOC, TIM_CHANNEL_1) != HAL_OK)
  {
    Error_Handler();
  }
  sBreakDeadTimeConfig.OffStateRunMode = TIM_OSSR_DISABLE;
  sBreakDeadTimeConfig.OffStateIDLEMode = TIM_OSSI_DISABLE;
  sBreakDeadTimeConfig.LockLevel = TIM_LOCKLEVEL_OFF;
  sBreakDeadTimeConfig.DeadTime = 0;
  sBreakDeadTimeConfig.BreakState = TIM_BREAK_DISABLE;
  sBreakDeadTimeConfig.BreakPolarity = TIM_BREAKPOLARITY_HIGH;
  sBreakDeadTimeConfig.AutomaticOutput = TIM_AUTOMATICOUTPUT_DISABLE;
  if (HAL_TIMEx_ConfigBreakDeadTime(&htim1, &sBreakDeadTimeConfig) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM1_Init 2 */

  /* USER CODE END TIM1_Init 2 */
  HAL_TIM_MspPostInit(&htim1);

}

/**
  * @brief TIM2 Initialization Function
  * @param None
  * @retval None
  */
static void MX_TIM2_Init(void)
{

  /* USER CODE BEGIN TIM2_Init 0 */

  /* USER CODE END TIM2_Init 0 */

  TIM_ClockConfigTypeDef sClockSourceConfig = {0};
  TIM_MasterConfigTypeDef sMasterConfig = {0};

  /* USER CODE BEGIN TIM2_Init 1 */

  /* USER CODE END TIM2_Init 1 */
  htim2.Instance = TIM2;
  htim2.Init.Prescaler = 4799;
  htim2.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim2.Init.Period = 99;
  htim2.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim2.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_Base_Init(&htim2) != HAL_OK)
  {
    Error_Handler();
  }
  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
  if (HAL_TIM_ConfigClockSource(&htim2, &sClockSourceConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim2, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM2_Init 2 */

  /* USER CODE END TIM2_Init 2 */

}

/**
  * @brief TIM3 Initialization Function
  * @param None
  * @retval None
  */
static void MX_TIM3_Init(void)
{

  /* USER CODE BEGIN TIM3_Init 0 */

  /* USER CODE END TIM3_Init 0 */

  TIM_ClockConfigTypeDef sClockSourceConfig = {0};
  TIM_MasterConfigTypeDef sMasterConfig = {0};
  TIM_OC_InitTypeDef sConfigOC = {0};

  /* USER CODE BEGIN TIM3_Init 1 */

  /* USER CODE END TIM3_Init 1 */
  htim3.Instance = TIM3;
  htim3.Init.Prescaler = 1881;
  htim3.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim3.Init.Period = 254;
  htim3.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim3.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_Base_Init(&htim3) != HAL_OK)
  {
    Error_Handler();
  }
  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
  if (HAL_TIM_ConfigClockSource(&htim3, &sClockSourceConfig) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_TIM_PWM_Init(&htim3) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim3, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sConfigOC.OCMode = TIM_OCMODE_PWM1;
  sConfigOC.Pulse = 0;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_HIGH;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  if (HAL_TIM_PWM_ConfigChannel(&htim3, &sConfigOC, TIM_CHANNEL_1) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM3_Init 2 */

  /* USER CODE END TIM3_Init 2 */
  HAL_TIM_MspPostInit(&htim3);

}

/**
  * @brief TIM4 Initialization Function
  * @param None
  * @retval None
  */
static void MX_TIM4_Init(void)
{

  /* USER CODE BEGIN TIM4_Init 0 */

  /* USER CODE END TIM4_Init 0 */

  TIM_ClockConfigTypeDef sClockSourceConfig = {0};
  TIM_MasterConfigTypeDef sMasterConfig = {0};
  TIM_OC_InitTypeDef sConfigOC = {0};

  /* USER CODE BEGIN TIM4_Init 1 */

  /* USER CODE END TIM4_Init 1 */
  htim4.Instance = TIM4;
  htim4.Init.Prescaler = 1881;
  htim4.Init.CounterMode = TIM_COUNTERMODE_UP;
  htim4.Init.Period = 254;
  htim4.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
  htim4.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
  if (HAL_TIM_Base_Init(&htim4) != HAL_OK)
  {
    Error_Handler();
  }
  sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
  if (HAL_TIM_ConfigClockSource(&htim4, &sClockSourceConfig) != HAL_OK)
  {
    Error_Handler();
  }
  if (HAL_TIM_PWM_Init(&htim4) != HAL_OK)
  {
    Error_Handler();
  }
  sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
  sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
  if (HAL_TIMEx_MasterConfigSynchronization(&htim4, &sMasterConfig) != HAL_OK)
  {
    Error_Handler();
  }
  sConfigOC.OCMode = TIM_OCMODE_PWM1;
  sConfigOC.Pulse = 0;
  sConfigOC.OCPolarity = TIM_OCPOLARITY_HIGH;
  sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
  if (HAL_TIM_PWM_ConfigChannel(&htim4, &sConfigOC, TIM_CHANNEL_1) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN TIM4_Init 2 */

  /* USER CODE END TIM4_Init 2 */
  HAL_TIM_MspPostInit(&htim4);

}

/**
  * @brief USART3 Initialization Function
  * @param None
  * @retval None
  */
static void MX_USART3_UART_Init(void)
{

  /* USER CODE BEGIN USART3_Init 0 */

  /* USER CODE END USART3_Init 0 */

  /* USER CODE BEGIN USART3_Init 1 */

  /* USER CODE END USART3_Init 1 */
  huart3.Instance = USART3;
  huart3.Init.BaudRate = 9600;
  huart3.Init.WordLength = UART_WORDLENGTH_8B;
  huart3.Init.StopBits = UART_STOPBITS_1;
  huart3.Init.Parity = UART_PARITY_NONE;
  huart3.Init.Mode = UART_MODE_TX_RX;
  huart3.Init.HwFlowCtl = UART_HWCONTROL_NONE;
  huart3.Init.OverSampling = UART_OVERSAMPLING_16;
  if (HAL_UART_Init(&huart3) != HAL_OK)
  {
    Error_Handler();
  }
  /* USER CODE BEGIN USART3_Init 2 */

  /* USER CODE END USART3_Init 2 */

}

/**
  * Enable DMA controller clock
  */
static void MX_DMA_Init(void)
{

  /* DMA controller clock enable */
  __HAL_RCC_DMA1_CLK_ENABLE();

  /* DMA interrupt init */
  /* DMA1_Channel3_IRQn interrupt configuration */
  HAL_NVIC_SetPriority(DMA1_Channel3_IRQn, 0, 0);
  HAL_NVIC_EnableIRQ(DMA1_Channel3_IRQn);

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
  __HAL_RCC_GPIOD_CLK_ENABLE();
  __HAL_RCC_GPIOA_CLK_ENABLE();
  __HAL_RCC_GPIOB_CLK_ENABLE();

  /*Configure GPIO pin Output Level */
  HAL_GPIO_WritePin(GPIOA, GPIO_PIN_4|GPIO_PIN_5|GPIO_PIN_9|GPIO_PIN_10, GPIO_PIN_RESET);

  /*Configure GPIO pin Output Level */
  HAL_GPIO_WritePin(GPIOB, GPIO_PIN_12|GPIO_PIN_4|GPIO_PIN_5, GPIO_PIN_RESET);

  /*Configure GPIO pin : PA0 */
  GPIO_InitStruct.Pin = GPIO_PIN_0;
  GPIO_InitStruct.Mode = GPIO_MODE_IT_FALLING;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pins : PA1 PA15 */
  GPIO_InitStruct.Pin = GPIO_PIN_1|GPIO_PIN_15;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pins : PA4 PA5 PA9 PA10 */
  GPIO_InitStruct.Pin = GPIO_PIN_4|GPIO_PIN_5|GPIO_PIN_9|GPIO_PIN_10;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(GPIOA, &GPIO_InitStruct);

  /*Configure GPIO pin : PB1 */
  GPIO_InitStruct.Pin = GPIO_PIN_1;
  GPIO_InitStruct.Mode = GPIO_MODE_IT_RISING;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

  /*Configure GPIO pin : PB2 */
  GPIO_InitStruct.Pin = GPIO_PIN_2;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

  /*Configure GPIO pin : PB12 */
  GPIO_InitStruct.Pin = GPIO_PIN_12;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

  /*Configure GPIO pins : PB13 PB8 PB9 */
  GPIO_InitStruct.Pin = GPIO_PIN_13|GPIO_PIN_8|GPIO_PIN_9;
  GPIO_InitStruct.Mode = GPIO_MODE_INPUT;
  GPIO_InitStruct.Pull = GPIO_PULLDOWN;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

  /*Configure GPIO pin : PB3 */
  GPIO_InitStruct.Pin = GPIO_PIN_3;
  GPIO_InitStruct.Mode = GPIO_MODE_IT_FALLING;
  GPIO_InitStruct.Pull = GPIO_PULLUP;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

  /*Configure GPIO pins : PB4 PB5 */
  GPIO_InitStruct.Pin = GPIO_PIN_4|GPIO_PIN_5;
  GPIO_InitStruct.Mode = GPIO_MODE_OUTPUT_PP;
  GPIO_InitStruct.Pull = GPIO_NOPULL;
  GPIO_InitStruct.Speed = GPIO_SPEED_FREQ_LOW;
  HAL_GPIO_Init(GPIOB, &GPIO_InitStruct);

  /* EXTI interrupt init*/
  HAL_NVIC_SetPriority(EXTI0_IRQn, 2, 0);
  HAL_NVIC_EnableIRQ(EXTI0_IRQn);

  HAL_NVIC_SetPriority(EXTI1_IRQn, 2, 0);
  HAL_NVIC_EnableIRQ(EXTI1_IRQn);

  HAL_NVIC_SetPriority(EXTI3_IRQn, 1, 0);
  HAL_NVIC_EnableIRQ(EXTI3_IRQn);

/* USER CODE BEGIN MX_GPIO_Init_2 */
/* USER CODE END MX_GPIO_Init_2 */
}

/* USER CODE BEGIN 4 */

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

#ifdef  USE_FULL_ASSERT
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
