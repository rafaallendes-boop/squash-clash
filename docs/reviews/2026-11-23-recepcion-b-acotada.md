# Recepción del saque B acotada: parada durante preparación

23 nov 2026. Base `90ddc7e275f23e0b8774c57e7db93f106eef5f90`. Encargo: `/Users/rafaallendes/sc-specs/impl-recepcion-b-acotada.txt`, incluida la aprobación del PO del 23 nov, 16:33, para medir 25/50 ms y después implementar el menor valor que mejore claramente el novato que toca sin romper N2. No fusionar.

**Estado: reanudado por instrucción explícita; medición pendiente del lock de Unity.** `/Users/rafaallendes/sc-specs/continua-106.txt` autoriza corregir el defecto nuevo sin tocar el test, confirma la volea opcional del saque en los tres niveles y deja los anillos para otra PR con Claude. No se eligió una tolerancia. La preparación del experimento queda aislada en `exp/serve-reception-margin`, worktree `sc-exp-recepcion-b`, y no se debe fusionar.

## Evidencia y defecto

Se añadió una entrada de margen explícito al contrato, inicialmente sin comportamiento, y tres pruebas de borde. Se ejecutaron con `scripts/test.sh`, lock `/Users/rafaallendes/sc-unity.lock` y una sola instancia de Unity. `scripts/device.env` se copió sin cambios del origen autorizado; SHA-256 `0b0c94db19e06f7de8a864991ed819fc984f16160f3a35b483d9aff9167cec2f`.

| Ejecución | Resultado fresco | Fuente local ignorada, en worktree experimental |
|---|---|---|
| RED `scripts/test.sh editmode ServeEarlyAllowance` | 3 tests, 0 aprobados, 3 fallidos por aserción: esperaba `Valid`, obtuvo `Failed` | `Logs/evidence/reception-b-preparation/red-results.xml`, `red.log` |
| Primera ejecución posterior, `scripts/test.sh editmode ContactContractTests` | 33 tests, 32 aprobados, 1 fallido por aserción; **no es GREEN** | `Logs/evidence/reception-b-preparation/first-green-results.xml`, `first-green.log` |

Falla `ServeEarlyAllowance_LatencyCompensationIsAppliedOnce`: una liberación de 1,045 s menos 70 ms de compensación debería estar en el borde de 25 ms anterior a K-in (1 s). El prototipo la rechaza como temprana. La aritmética de doble precisión da `0.974999999999999867`, frente al borde `0.974999999999999978` (diferencia de −1,11 × 10⁻¹³ ms). Es evidencia de un defecto en la comparación del borde del prototipo, no de que el caso pase. No se cambió la prueba ni se corrigió el contrato después de detectarlo.

Se aplica AGENTS.md §2: «Ante un defecto o sorpresa: registrar y detenerse, sin corregir ni reintentar para obtener Pass». Evidencia preservada; `device.env` eliminado al terminar y lock liberado.

**Continuación autorizada:** se reprodujo el fallo (3 tests: 2 aprobados, 1 fallido), sin cambiar la prueba ni su aserción. Se corrigió la comparación inclusiva expresando el borde corregido en el reloj original del evento: `release >= (KInReal − marginMs/1000) + latencyMs/1000`. La liberación corregida sigue calculándose una sola vez; no se agrega un epsilon. `ContactContractTests`: **33/33**, XML fresco `Logs/evidence/reception-b-preparation/resume-green-results.xml` (ignorado, worktree experimental). Commit experimental `b4cccbd`; protocolo previo a datos en `docs/validation/2026-11-23-recepcion-b-protocolo.md` de esa rama. La copia temporal de `device.env` se retirará al terminar esta continuación.

## Mediciones y puertas pendientes

| Medida solicitada | Base | 25 ms | 50 ms |
|---|---|---|---|
| M1 oficial, novato std | Sin medir (n=0) | Sin medir (n=0) | Sin medir (n=0) |
| Devolución del novato que toca | Sin medir (n=0) | Sin medir (n=0) | Sin medir (n=0) |
| Puntos, partidos y volea al saque por nivel | Sin medir (n=0) | Sin medir (n=0) | Sin medir (n=0) |
| N2, `ConfirmPerStyle` | No evaluado | No evaluado | No evaluado |

No se ejecutaron las suites completas EditMode/PlayMode, AutoQA ni check-release. Los resultados de esta preparación no autorizan seleccionar valor, implementar ni fusionar. No cambian física, solver, `DefaultPreset.json`, SimulationKey, RallyParams ni sus hashes en la rama de la PR.

**Arquitectura (Claude Opus, solo lectura): apta con condiciones.** Recomienda un margen explícito fuera de `TimingSettings`, con valor en `RallyParams`, y fijar la recepción en `RallyDirector.LaunchToHuman` desde `committed`, antes del lanzamiento. No leer `Rules.Serving` al soltar: se apaga al primer bote. La continuación explícita resuelve el borde inclusivo y confirma el alcance sobre el golpe temprano del humano al recibir el saque, incluida la volea opcional. Falta revisar la implementación final. Resultado de arquitectura: tarea `task_95798afb7538`, dispatch `ctx_c20be7b3fe18`.

**Textos:** ninguno nuevo o modificado en el producto. Los anillos y «Espera el bote» siguen pendientes; corresponden a Claude Sonnet con `/frontend-design`, según AGENTS.md.
