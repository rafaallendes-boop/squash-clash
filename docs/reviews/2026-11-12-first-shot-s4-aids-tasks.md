# First Shot T4.2 — ayudas y tareas provisionales

Base main/origin/main `ef9be56b382936fe948217c0efe4ee02cce28b4c`. Encargo literal guardado primero en `1f9c8fb`, en el registro de aprobaciones. D1 opción B, D2 bandas de observación y D4 correcciones autorizadas; diseño de origen: [Step 0 T4.2](2026-11-11-first-shot-s4-timing.md#t42--step-0-documental-implementación-no-autorizada). Sin aceptación de First Shot, sin sesión física/4.5 ni capa causal.

## Hashes y anclajes autorizados por D1

Cálculo Unity desde DefaultPreset.json real, prueba TimingAidConfigTests; original XML/log local en Logs/evidence/s4-aids-tasks/config-green. No hashes predichos.

| Configuración | Antes (107 claves) | Después (108 claves) |
|---|---|---|
| Defaults / SL1 | c0d8ec7050e0 | c0e9f00176d5 |
| SL5 (practice.launch=4) | 55a85f500cf1 | 1877d083c345 |
| P0 efectivo (overlay legacy/repeat=3) | 6ecfdfa78cbb | **42d9efbf50fc** |
| Proyección defaults 41 heredados | 0d39e6016bd8 | 0d39e6016bd8 |
| SimulationKey | 166a96e319c7 | 166a96e319c7 |

Única clave nueva `timing.aidLevel=0`. Ningún valor existente cambia. P0 no se promueve ni reescribe. Para futuros prechecks, usar el nuevo P0 efectivo **42d9efbf50fc**, no el histórico.

Commit separado de anclajes: Scenario.cs (SL1/SL5), autoqa-check v7 en .claude/agents/autoqa-operator.md y referencias de hash de regresión/experimentos. El skill autoqa invoca ese mismo v7 y no contiene hashes duplicados. No cambian escenarios, expectativas, márgenes ni tolerancias. Los tres anclajes de hash de tests de rally se actualizan únicamente por la nueva clave, sin cambiar fixtures ni comportamiento de rally.

## Bordes y decisiones de representación

D4: contornos exteriores para bordes tempranos A/V, interiores para tardíos. Tras T* el anillo continúa hacia dentro; alpha constante, sin extinguirse en +V inclusivo. T3 conserva la supresión de ambos contornos V. El contrato/ImpactCue/ventanas no se modifican.

D1: nivel capturado por recepción; parcial oculta anillo/triángulo/cuatro contornos desde T*−150 ms y no reaparece por replan. Se registra target/versión que activó el corte, planes posteriores y nivel. Ninguna suprime solo timing; no toca mira/potencia/feedback.

D2: intervalos cerrados por abajo/abiertos por arriba, último extremo incluido, sin epsilon ni clamp de zonas. X del bote usa los mismos cortes exactos 0,20 / 2,20 / 4,20 / 6,20. Esta política de bordes es elección simple de implementación según el encargo, **no aprobación adicional del PO**. Las zonas/bandas son observación y no umbrales de aceptación.

## TDD y validaciones

- D4: RED 15/17 (aserciones de +V y contorno tardío), GREEN 17/17.
- D1 vista: RED 17/19 (parcial/ninguna), GREEN 19/19.
- D1 catálogo: RED 0/1 (clave ausente), GREEN 1/1.

Implementación de tareas y verificaciones finales pendientes; este informe es incremental y no declara entrega completa.
