# Rival frente al golpe corto — I1–I3, bloqueado por M3

22 nov 2026 · RAL-174 · Codex · base `518f9c3` · PR #97 · código validado `5d77420`.

**I1–I3 implementados, pero no aprobados para fusión ni build.** EditMode completo: **978 tests, 977 Passed, 1 Failed, 0 Skipped**. El único fallo está en un test existente de identidad/calibración de estilos; se detuvo sin ajustar código para recuperar esa métrica ni cambiar criterios. PR en borrador.

## Resultado y bloqueo

`BoastRivalTests.On_InPlay_TheFondistaBoastsLessFromTheBackQuarterThanTheDefinidor`, línea 81, exige Fondista < Definidor en proporción de dos paredes entre contactos desde el cuarto de fondo:

| Fondista | Definidor | Criterio |
|---|---|---|
| **3,9906103286384977 %** | **1,4705882352941176 %** | Fondista < Definidor: **FAIL** |

Fracciones exactas del XML: `0.039906103286384977` y `0.014705882352941176`. Son resultados de esta rama, no un Δ R7 frente a main. Test, semillas, partidos, pesos, presets, umbrales y comparación permanecen intactos. No se reintentó para obtener Pass.

PlayMode completo, AutoQA V/T3/volea y check-release quedan pendientes por esta parada. El PlayMode dirigido de I1 sí pasó **2/2**. R7 y Anexo A no medidos.

## Cambio y TDD

- I1: aproximación máxima después del primer bote, hasta el segundo, sobre el camino congelado. Un intento visual antes del fin del punto; sin intento si cerró antes del primer bote. RED: paso 103 frente a −1 y evento 1 frente a 0. GREEN: **2/2 EditMode y 2/2 PlayMode**; sin cambiar resultado ni métricas.
- I2: `stretched` permite recta/cruzado × alto × media, sin dos paredes. RED: **65/144/112** dos paredes en 500 decisiones por estilo frente a 0. GREEN dirigido: **11/11**. La presión `planned` conserva sus opciones.
- I3: el predictor y planificador compartidos descartan intenciones no alcanzables desde la posición observada del humano. Sin ninguna alcanzable, menor retraso y marca `LastStretchedFallback`. RED: **62/100** elecciones inalcanzables por estilo frente a 0; respaldo 46 pasos frente a 27. GREEN dirigido: **15/15**; R2/R3 en 0 en los fixtures. Los 17 tests nuevos de EditMode pasaron en la regresión completa.

Huella de 1.080 decisiones `planned` idéntica a la captura previa a I2/I3: `53870c765e9e1cc410b87b41d810afc7fbe53ac0ffa42345b9b97192786de4de`. Hashes verificados: defaults `207da8f2c636`, RallyParams `a82c34b661bf`, SimulationKey `166a96e319c7`.

## Autorización y límites

SC-PM, 22 nov 19:15 −03: «Mi condición anterior fue demasiado estricta y la retiro». Se corrigieron los errores propios de compilación y la suposición de altura fija del test nuevo de I3, manteniendo el escenario de la spec, oponente en la T, predictor y expectativa 0. Detalle y autorización: [evidencia TDD](../validation/2026-11-22-rival-short-min-tdd.md). Logs/XML ignorados: `Logs/evidence/rival-short-min/`.

I2 cambia la disponibilidad de dos paredes e I3 las elecciones y secuencia RNG desde `stretched`; el fallo M3 materializa ese riesgo. I4 y P1–P6 fuera. Sin cambios en tests existentes, presets, física, solver, contrato, reloj, humano, FondoAi, `pullFullMm` o calibración. Todas las corridas adquirieron y liberaron su candado. Sin fusión, instalación ni acceso al iPhone.
