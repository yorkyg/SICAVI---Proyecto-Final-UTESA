# Guía de uso - SICAVI Cámara + STM32

## 1. Preparar Visual Studio

1. Abre **Visual Studio Installer**.
2. Selecciona **Modificar** en Visual Studio 2022.
3. Marca **Desarrollo de escritorio de .NET**.
4. En componentes individuales, confirma que esté instalado el **SDK de .NET 8**.
5. Reinicia Visual Studio después de instalar componentes.

## 2. Abrir el proyecto

1. Descomprime el paquete completo de integración en una carpeta normal, por ejemplo:

   ```text
   D:\GENERAL\Proyectos\SICAVI_Integracion_STM32_CSharp
   ```

2. No ejecutes la solución directamente dentro del archivo ZIP.
3. Abre `SICAVI.sln`.
4. Espera a que desaparezca el mensaje **Restaurando paquetes NuGet**.
5. En la barra superior selecciona:

   ```text
   Debug | x64
   ```

6. En el Explorador de soluciones, confirma que `Sicavi.WinForms` esté en negrita. Si no lo está, haz clic derecho y selecciona **Establecer como proyecto de inicio**.
7. Presiona `Ctrl + Shift + B` para compilar.
8. Presiona `F5` para ejecutar.

## 3. Iniciar sesión

En la primera ejecución, SICAVI crea la base de datos y muestra el formulario de acceso antes del panel principal.

```text
Usuario: admin
Clave:   Sicavi@123
```

El botón **Registrar nuevo operador**, visible en el formulario de inicio de sesión, crea cuentas de operación.

El botón **GESTIÓN DE USUARIOS** está siempre visible en `01 · ACCESOS`, al inicio del panel lateral del formulario principal. Al iniciar con una cuenta Administrador permite crear usuarios, editar roles, activar/desactivar cuentas y cambiar claves. Si la sesión pertenece a un Operador, el botón informa que se necesita permiso de administrador.

El botón **CERRAR SESIÓN** está debajo de **GESTIÓN DE USUARIOS**. Envía una detención de seguridad, libera la cámara y el puerto serie, cierra el formulario principal y vuelve al login para ingresar con otra cuenta. La X de la ventana cierra completamente SICAVI.

## 4. Primera prueba: visualizar la webcam

1. Cierra Teams, Zoom, OBS, la aplicación Cámara y cualquier programa que pueda estar usando la webcam.
2. Ejecuta SICAVI.
3. Confirma que la NUCLEO esté configurada como `COM6`. SICAVI la conecta automáticamente.
4. Presiona **INICIAR**. La cámara 0 abrirá automáticamente a `1920 x 1080`; nunca se abre por recibir solamente un estado del STM32.
6. Debes ver video y un rectángulo azul/cian en el centro. Ese rectángulo es la **ROI**, la zona que se analiza.
7. Comprueba que el valor de FPS sea estable.
8. Presiona **DETENER** y comprueba que la cámara se cierre automáticamente.

## 5. Preparar las muestras

Usa etiquetas blancas con una sola figura roja o verde, como las muestras físicas del proyecto:

- Círculo.
- Cuadrado.
- Triángulo.

Puedes imprimir [assets/plantilla_formas.svg](assets/plantilla_formas.svg). La figura debe quedar completamente visible, con buen contraste y sin tocar los bordes de la etiqueta.

Posición recomendada:

- Cámara mirando casi perpendicularmente hacia abajo.
- Caja centrada dentro de la ROI.
- Etiqueta en la cara superior de la caja.
- Luz uniforme, sin reflejos ni sombras duras.
- Fondo diferente del verde y el rojo de las muestras.

## 6. Probar color y forma

1. En **Color esperado**, selecciona `Verde`, `Rojo` o `Cualquiera`.
2. En **Forma esperada**, selecciona `Círculo`, `Cuadrado`, `Triángulo` o `Cualquiera`.
3. Pulsa **INICIAR**. El sistema transportará las cajas y mantendrá la vista previa activa.
4. Cuando el sensor `D8 / PA9` detecte una caja, el STM32 la posicionará y estabilizará.
5. SICAVI capturará y analizará automáticamente un fotograma; no se requiere un botón de análisis manual.

El resultado será:

| Estado | Significado |
|---|---|
| `ACEPTADA` | Color y forma coinciden con lo esperado. |
| `RECHAZADA` | El algoritmo detectó ambos, pero uno o ambos no coinciden. |
| `INDETERMINADA` | No existe suficiente evidencia para decidir. |

No se fuerza una decisión cuando la imagen es mala. Una forma desconocida no debe disfrazarse de círculo por entusiasmo algorítmico.

## 7. Ajustar HSV y formas

Los parámetros se encuentran en:

```text
src\Sicavi.WinForms\vision-settings.json
```

Después de editarlo, guarda, recompila y vuelve a ejecutar.

Parámetros principales:

| Campo | Función |
|---|---|
| `green` | Límites HSV del verde. |
| `redLow` y `redHigh` | Dos rangos necesarios para detectar rojo. |
| `minimumColorPercentage` | Porcentaje mínimo de píxeles para aceptar un color. |
| `minimumShapeAreaPixels` | Elimina contornos pequeños y ruido. |
| `polygonEpsilonRatio` | Controla la aproximación del contorno a un polígono. |
| `minimumCircleCircularity` | Circularidad mínima para aceptar un círculo. |
| `roi...Ratio` | Posición y tamaño proporcional de la ROI. |
| `minimumColorObjectAreaRatio` | Descarta manchas pequeñas antes de decidir el color. |
| `shapeMinimumSaturation` / `shapeMinimumValue` | Separan la figura coloreada de la caja blanca. |
| `maximumAnalysisWidth` | Reduce internamente la imagen para acelerar el análisis sin cambiar la vista capturada. |

Después de una inspección automática, la imagen anotada y la máscara quedan fijas. Se reemplazan únicamente cuando `D8 / PA9` detecta la siguiente caja.

Empieza ajustando solamente `minimumColorPercentage`. Cambiar diez parámetros a la vez produce una calibración muy creativa y poco repetible.

## 8. Consultar producción, horómetro y alarmas

Pulsa **Datos de producción** en la parte superior del panel principal. Selecciona el rango de fechas y pulsa **Actualizar**. Las pestañas contienen:

- Clasificaciones y conteos de buenas, malas, indeterminadas y total.
- Horómetro diario, horómetro histórico total y ciclos del expulsor.
- Alarmas con fecha/hora de inicio, cierre, código y estado.

## 9. Errores comunes

### La cámara no abre

- Cierra otras aplicaciones que usen video.
- Revisa **Configuración de Windows > Privacidad y seguridad > Cámara**.
- Permite acceso a aplicaciones de escritorio.
- Desconecta y vuelve a conectar la webcam.
- Verifica que la webcam acepte `1920 x 1080`.
- Si el índice 0 corresponde a la cámara de la laptop, cambia la cámara predeterminada desde la configuración de Windows o deshabilita temporalmente la cámara integrada.

### Aparece `OpenCvSharpExtern.dll` o falta una DLL

- Verifica que la plataforma sea `x64`.
- Limpia y recompila la solución.
- Restaura los paquetes NuGet.
- Instala o repara **Microsoft Visual C++ Redistributable 2015-2022 x64**.

### Detecta mal el color

- Evita iluminación amarilla intensa.
- Acerca la caja para que ocupe buena parte de la ROI.
- Comprueba la máscara de color en el panel derecho.
- Ajusta HSV con fotografías tomadas en la maqueta real.

### No reconoce la figura

- Usa figura negra sobre etiqueta blanca.
- Evita figuras pequeñas.
- Mantén la figura completa dentro de la ROI.
- Evita arrugas, sombras y reflejos.
- Revisa el contorno azul del panel derecho.

### SQL Server no abre la base SICAVI

- Verifica que la instancia indicada en `database-settings.json` esté iniciada.
- La configuración incluida usa `.\WINCC` y autenticación de Windows.
- Ejecuta una primera vez con un usuario que tenga permiso para crear bases.
- Consulta `docs\BASE_DE_DATOS_Y_USUARIOS.md`.

## 10. Orden recomendado para documentar las pruebas

1. Captura de la interfaz con cámara conectada.
2. Foto de cada muestra.
3. Resultado con verde + círculo.
4. Resultado con rojo + triángulo.
5. Ejemplo de rechazo por color.
6. Ejemplo de rechazo por forma.
7. Ejemplo indeterminado.
8. Tabla de problemas encontrados y ajustes aplicados.

## 11. Conectar la NUCLEO-G071RB

1. Carga primero el firmware incluido en la carpeta `STM32` del paquete principal.
2. Conecta la NUCLEO por el USB del ST-LINK.
3. En el Administrador de dispositivos asigna `COM6` a `STMicroelectronics STLink Virtual COM Port`.
4. Abre SICAVI y espera a que la barra inferior indique la conexión automática de COM6.
5. Verifica que se actualicen `Estado`, `LDR`, `Horómetro`, `Expulsor` y `Alarma`.
6. Pulsa **INICIAR**; solamente entonces se inicia la cámara 0. Al pulsar **DETENER**, se detiene.

Cuando el sensor conectado a `D8 / PA9` detecta una caja, el STM32 completa el avance mecánico configurado de 400 ms, detiene la banda y espera 300 ms. Luego el programa reserva un fotograma nuevo exclusivamente para esa caja, lo analiza y devuelve el resultado al STM32. Para conexiones, protocolo y pruebas físicas consulta la carpeta `docs` de la raíz del repositorio.
