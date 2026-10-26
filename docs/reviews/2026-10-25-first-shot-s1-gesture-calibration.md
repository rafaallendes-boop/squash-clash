# Calibración experimental del gesto S1

Autorización PO del 2026-10-25. Rama `firstshot/s1-gesture-calibration`, base main e11cc3aaa9dad7112d886e43d990732e0e198b43. S1 no aceptado; S2–S4/T8 no autorizados. Este registro se escribe y se commitea **antes de calcular B**. Las bandas siguientes son instrumentos experimentales de esta comparación; no modifican criterios de la spec ni del plan.

## Criterio previo a los resultados de B

Se comparan los mismos 40 intentos, incluidos ceros y conservadores. A usa **sin recalcular** la tabla aprobada de [PR #21](2026-10-25-first-shot-s1-power-calibration-proposal.md): 500/0/3/51, flick 250. No hay gestos medios registrados. Se declaran manos mezcladas, orden 30→20 y DPI sin regla como límites.

Bandas experimentales sobre P normalizada: **suave [0, 0,25), media [0,25, 0,60), fuerte [0,60, 1]**. Un cuarto de escala deja margen a variación fina; la banda media ocupa el centro y fuerte comienza claramente por encima de media escala, sin exigir 100 %. Cero se conserva en la banda suave para clasificar todos los intentos, pero una mediana suave cero no acredita control de intensidad. Estas bandas no son zonas de H2 ni criterios de aceptación de S1.

Requisitos por orden, fijados antes de B:

1. Orden: mediana suave < mediana fuerte.
2. Separación: distancia entre medianas ≥ 0,35 y cuartil 25 fuerte > cuartil 75 suave, incluyendo ceros; percentiles por interpolación lineal del índice (n−1)p.
3. Rango útil: mediana suave **> 0 y < 0,25**, mediana fuerte **≥ 0,60**. Se informará también saturación; más de 20 % de fuertes en 100 % indica escala demasiado corta. No hay evidencia para comprobar la banda media antes de una sesión futura.
4. Salidas conservadoras: como máximo 20 % por serie agregada suave/fuerte. Se exige apoyo temporal y no se reclasifica un rechazo como potencia medida para pasar el requisito.
5. Ningún fuerte accidental: buscar en los 138 registros un arrastre rápido seguido de liberación suave. Las trazas no etiquetadas solo permiten detectar **compatibilidad cinemática**, no intención; ausencia de casos representativos deja este requisito **NotEvaluated**, nunca Pass. Una contraprueba cinemática con P fuerte es un riesgo que impide dar por segura la elección. No se inventarán intenciones ni nuevos datos.
6. Simplicidad: si ambos cumplen y la diferencia práctica es ≤ 0,05 en medianas, ≤ 10 puntos porcentuales en rechazo y ninguna ventaja demostrada en seguridad, gana A.

Solo se implementará un método que resulte defendible con estos requisitos; si ambos se descartan o la seguridad no permite elegir, se documenta y se detiene antes de código/build/sesión, para decisión del PO. No se reajustan bandas para forzar un resultado favorable.

## B propuesto antes de calcular

Pico suavizado causal con **tau de suavizado 20 ms**, **decaimiento exponencial 120 ms**, escala **deadZone=10 / full=400 mm/s**, exponente 1. Cada intervalo positivo de duración ≤ 51 ms aporta velocidad escalar distancia/tiempo. Dentro de cada tramo de muestras consecutivas fiables, suavizado `s += (1−exp(−dt/20ms))*(v−s)`. Solo tramos con ≥ 3 muestras y ≥ 30 ms pueden aportar un pico. `q = max(q*exp(−dt/120ms), s)` mientras hay apoyo; fuera de un tramo fiable q solo decae con tiempo real y el suavizado se reinicia. En liberación se aplica el decaimiento hasta su timestamp y `P=clamp((q−10)/390,0,1)`. Sin tramo con apoyo, salida conservadora P=0. El decaimiento incluye las pausas aunque no lleguen muestras. No se exige que el soporte termine justo en la ventana final: esa es la diferencia experimental con A.

Se calculará el candidato fijo y una sensibilidad **predeclarada**, sin seleccionar nuevas constantes después: full ∈ {300, 400, 500}, deadZone ∈ {0, 10, 20}, decaimiento ∈ {60, 120, 180} ms, tau=20 ms; soporte 3/30/51 sin cambios. El objetivo es comprobar si alguna escala razonable de este B alcanza rango útil sin exponer fuertes al apuntar rápido y soltar suave. Se publican todas las combinaciones, no solo la favorable. No se amplía la búsqueda para hacer pasar el criterio.

## Estado

Pendiente calcular B, buscar el caso de riesgo y decidir antes de implementar. Sin datos nuevos ni contacto con el iPhone.
