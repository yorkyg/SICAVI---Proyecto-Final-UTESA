# SICAVI — Corrección de GPIO, reinicio/inicio y visión

Se corrigieron las dos fallas operativas reportadas: alarmas que no se reiniciaban y arranques que a veces no ocurrían. También se modificó la visión para trabajar con la figura dentro de la tarjeta, no con el borde de la caja ni con la banda.

## Qué quedó corregido

- La biblioteca `gpio.h/.c` del STM32 conserva ahora la estructura de `Mi libreria defaul`, incluida la validación de punteros y los retornos de error.
- D8 está definido como PA9, activo en bajo, con pull-up e interrupción descendente.
- START y STOP activos en bajo tienen pull-up para no quedar flotantes.
- El STM32 puede conservar ocho comandos UART completos. Antes podía perder `RESET` o `RUN` si coincidían con el latido.
- SICAVI espera la confirmación real de RESET antes de enviar RUN; ya no depende de una pausa fija de 150 ms.
- Si D8 permanece en 0 V, aparece una explicación concreta y el sistema no simula un arranque breve.
- La tarjeta blanca se localiza automáticamente y se analiza solo su interior.
- Un resultado indeterminado se comprueba con dos fotogramas adicionales; ambos deben coincidir para sustituir el primero.

## Resultado de pruebas

- C# Release: 0 errores, 0 advertencias.
- Firmware STM32: compilación correcta y ELF generado.
- Captura real verde/círculo: 31.95 % verde, circularidad 0.866, resultado aceptado.
- Regresión sintética: 6/6 combinaciones correctas.

## Lo que falta medir en la máquina

La calibración definitiva necesita las seis piezas reales. Deben probarse, idealmente tres veces cada una:

1. círculo verde;
2. círculo rojo;
3. cuadrado verde;
4. cuadrado rojo;
5. triángulo verde;
6. triángulo rojo.

Para cada muestra se guardarán la imagen, porcentaje de rojo/verde, vértices y circularidad. Los umbrales se ajustarán después de reunir la serie completa para no optimizar una pieza perjudicando otra.

## Referencias técnicas

El mapeo D8=PA9 está documentado en [UM2324 de ST](https://www.st.com/resource/en/user_manual/dm00452640.pdf). El uso de HSV 8-bit con H en 0..180 y la aproximación de contornos siguen la documentación oficial de [conversiones de color](https://docs.opencv.org/4.13.0/d8/d01/group__imgproc__color__conversions.html) y [descriptores de forma](https://docs.opencv.org/4.13.0/d3/dc0/group__imgproc__shape.html) de OpenCV.

Los resultados completos y los comandos de reproducción están en el [informe STM32](INFORME_FINAL_STM32.md), el [informe C#](INFORME_FINAL_CSHARP.md) y el [plan de pruebas](PRUEBAS_PASO_A_PASO.md).
