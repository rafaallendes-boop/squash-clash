# Rival frente al golpe corto — incremento mínimo I1–I3

22 nov 2026 · RAL-174 · Codex · base `518f9c3` · rama `feat/rival-short-min` · PR #97.

**Estado: I1–I3 implementados; gates completos en curso.** PR en borrador, sin fusión ni instalación. Spec de #92 (`d320c13`) leída con `git show`; el encargo explícito incluye I3 aunque la adenda HOY lo difiere.

## Cambio

- I1: una aproximación máxima después del primer bote, hasta el segundo, sobre el camino congelado del rival. Sin intento si el punto cerró antes del primer bote. El director emite una sola estirada fallida antes de terminar el punto, sin cambiar su resultado ni las métricas.
- I2: `stretched` solo permite recta/cruzado × alto × media, sin dos paredes. La presión con plan `planned` conserva las opciones anteriores.
- I3: el predictor y planificador compartidos filtran las intenciones no alcanzables por el humano desde su posición observada. Sin ninguna alcanzable, se elige el menor retraso y se marca `LastStretchedFallback`.

## TDD y autorización

SC-PM, 22 nov 19:15 −03: «Mi condición anterior fue demasiado estricta y la retiro». Autoriza corregir e iterar sobre compilación, parche y suposiciones de tests nuevos. Se mantienen las paradas ante cambios necesarios en criterios protegidos, entorno o comportamiento inesperado ya presente en main. Registro y resultados detallados: [evidencia TDD](../validation/2026-11-22-rival-short-min-tdd.md).

- I1 puro: RED 103 frente a −1; GREEN **2/2**. Guardia de cierre anterior al bote: verde desde el esqueleto.
- I1 director: RED 1 evento frente a 0, con plan `unreachable` confirmado; GREEN **2/2**. Recepción con golpe: guardia verde.
- I2: RED **65/144/112** dos paredes en 500 decisiones por estilo, frente a 0; GREEN **11/11**, incluidas guardias `planned`.
- I3: RED **62/100** elecciones inalcanzables por estilo, frente a 0; respaldo 46 pasos frente a 27. GREEN **15/15** con 0 violaciones en los casos mixtos y respaldo marcado.
- La búsqueda inicial del test nuevo suponía altura 0,8 m y no encontró el caso mixto; se corrigió buscando dentro de la ventana legal de la spec, conservando al humano en la T y la expectativa 0. Los dos errores de compilación previos y su autorización constan en la evidencia; nunca se contaron como RED o GREEN.

## Límites y riesgo

Huella de 1.080 decisiones `planned` idéntica a la previa a I2/I3: `53870c765e9e1cc410b87b41d810afc7fbe53ac0ffa42345b9b97192786de4de`. Hashes verificados en los tests: defaults `207da8f2c636`, RallyParams `a82c34b661bf`, SimulationKey `166a96e319c7`.

I2 puede mover la cuenta de dos paredes; I3 cambia elecciones y la secuencia RNG desde una recepción estirada. R7 y las distribuciones del Anexo A aún no medidos; R1–R3 se afirman solo para los fixtures verificados. Quedan para el iPhone la lectura visual y el p95.

I4 y P1–P6 fuera. Sin cambios en tests existentes, parámetros de preset, física, solver, contrato, reloj, humano, FondoAi, `pullFullMm` ni calibración. Todas las corridas toman y liberan su propio candado de Unity.
