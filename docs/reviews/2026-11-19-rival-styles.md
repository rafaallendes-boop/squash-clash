# Estilos del rival, volea del rival y cobertura: informe de entrega

19 nov 2026 · Rama `claude/rival-styles`, base `f068cee` (integración de #84), head `197629f`. Lo implementó un subagente Opus dirigido por Claude (product lead, delegación del PO del 18 nov); **la revisión de Claude no es independiente.** Diseño aprobado: [2026-11-19-rival-styles-design.md](../superpowers/specs/2026-11-19-rival-styles-design.md). Sin push, fusión ni instalación.

## Qué cambia

- **`RivalAi` con tres estilos:** «Fondista», «Definidor» y «Completo».
  - **Qué cambia el estilo:** pesos de la utilidad, aversión al riesgo, umbral de defensa, apertura del drop, disposición a volear, espera W y cobertura tras un golpe corto propio.
  - **Qué no cambia:** física, reglas, alcance, ventanas, error e información.
  - La dificultad sigue siendo seis parámetros por nivel, compartidos por los tres estilos.
  - `FondoAi` no se toca. El Fondista sin volea ni cobertura decide igual que `FondoAi` y reproduce las firmas congeladas de RallySim.
- **Volea del rival:**
  - Usa la misma oportunidad que el humano: un solo softmax sobre 36 opciones y un solo intento.
  - El soltar apuntado a la volea se juzga con `OpportunityContract.Select` (`RallyContact.ResolveAimedVolley`).
  - Hay una `CausalReference` por oportunidad, y la volea cuesta `VolleyEnergyCost`.
- **Cobertura:** si el bote predicho del golpe propio cae antes de 3,5 m, Fondista y Completo vuelven a la T y el Definidor a 1,5 m detrás del bote. Usa solo la pelota recién golpeada y el predictor compartido.
- **Juego real:**
  - Estilo al azar en cada partido del menú, reproducible mediante el contador `sc.firstRun.rivalMatches`.
  - La revancha conserva el rival; en Rally, o fuera del menú, juega el Fondista.
  - Se muestra debajo de «RIVAL» y en la línea del primer punto («Rival: Definidor · …»).
  - DEV «Estilo del rival»: Auto, Fondista, Definidor o Completo.
  - `style` en `rally-point`.
- **Decidido antes de calibrar:** el sesgo de volea del Completo pasa de 0 a −0,8.
- **Pendiente:** la pose de volea de la figura del rival.

## Tests editados (consecuencia directa)

1. `VolleyEnergyTests` y `VolleyRallyTests`: las proyecciones excluyen también `Style*`; sus valores no cambian.
2. `ZRallyHudTests.MatchButton…`: la línea del primer punto empieza con «Rival: Fondista · ».
3. `ZImpactMarkerAlignmentTests.TimingTriangle…`: compara con `controller.ShownPlan`, porque con volea el triángulo se dibuja en P\* de la volea. Diferencia: 0,000 cm.
4. `ZFirstRunFixture`: limpia la clave del contador.

`RallyDirector` agrega `AutopilotLine` y `AutopilotPower`, que solo usa un test.

## Compuertas (`197629f`)

- **RED por aserción:** EditMode 10 de 12 y PlayMode 3 de 3 contra stubs.
- **EditMode:** 882/882.
- **PlayMode:** 668/668.
- **AutoQA:** v, t3 y volley PASS/0/1, sin dirty (`20261119T193826Z-197629f-v-8821`, `20261119T193845Z-197629f-t3-2828`, `20261119T193903Z-197629f-volley-9130`).
- **check-release:** dev y prod OK (build 956).

## Hashes

- **RallyParams:** `5cdbb01e4621` → **`85441c9b1cd7`**.
  - Sin `Style*`: `5cdbb01e4621`.
  - Sin `Style*` ni `VolleyEnergyCost`: `6a7b329a7b85`.
  - Sin `Style*` ni `Volley*`: `927e304bbfec`.
- **Sin cambios:** defaults `207da8f2c636`/132, SimulationKey `166a96e319c7` y anclas de AutoQA.

## Calibración `search01`: detenida en los tres niveles

Objetivos fijados antes de medir: novato 60/40/20 %, ±4 pp en el promedio y ±8 pp por estilo; en Difícil, bot30 ≤ 45 %. En `search01` (200 partidos por celda, k de 1 a 3 sobre los cinco ruidos, temperatura fija) **ningún k cumple**.

Puntos del novato con el ruido actual, % [IC 95 %]:

| | Fondista | Definidor | Completo |
|---|---|---|---|
| Fácil | 38,9 [36,9–40,9] | 70,1 [68,0–72,1] | 41,8 [39,8–43,9] |
| Medio | 20,1 [18,3–22,1] | 72,9 [70,9–74,9] | 29,3 [27,3–31,3] |
| Difícil | 4,7 [3,7–5,9] | 72,0 [69,9–73,9] | 18,1 [16,4–20,0] |
| Difícil, bot30 | 1,6 | 0,2 | 3,9 |

**Causa:**
- El Definidor da 70–87 % con cualquier k contra el novato, y bot30 le gana solo el 0,1–3 % de los puntos.
- Su W adelantada y su cobertura de 1,5 m pierden contra golpes largos y fuertes, y matan los cortos de potencia media.
- El Fondista y el Completo sí siguen al ruido:
  - Fácil, k = 1,5: 59,4 y 63,1 %.
  - Medio, k = 1,5: 33,0 y 42,0 %.
  - Difícil, k = 1,25: 5,7 y 20,2 %.

**Identidad de los estilos, en todas las celdas:** Fondista 82–96 % largo; Definidor 43–55 % de ataque y volea en el 9,5–12 % de sus golpes; Completo con volea en el 0,3–1,5 %.

**Decisión de Claude (19 nov):** se acepta la propuesta del agente. El Definidor pasa a W en la T y cobertura «a la T», conserva sus pesos de ataque y volea, y se repite la búsqueda con el mismo método y objetivos. Evidencia: `docs/validation/evidence/2026-11-19-rival-styles/calibration-search01/`.

## Observaciones

- **SF-01:** sin corregir.
  - En el juego real el punto termina limpio: el error se cuenta, la pelota sigue hasta el doble bote y el punto es del otro jugador.
  - Hay un caso en las métricas reales del PO: build `22bb2d5`, punto para el Rival con recepción `SystemFault`.
- **Siguiente pasada:** que los atributos físicos afecten también a las IA.
