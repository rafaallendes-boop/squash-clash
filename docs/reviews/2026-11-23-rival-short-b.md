# RAL-174 — camino B, detenido por precondición B3

23 nov 2026 · Codex · rama `feat/rival-short-i2i3` · main `230b758` incorporado con merge `2bf90f6`, sin reescribir historial ni force push.

**Incompleto; PR en borrador.** Antes del GREEN, la escena predeclarada B3 de la spec #92 (`77544cf`) no cumple su precondición: debe ofrecer alguna dos paredes que el humano no alcance. Las cuatro son `Good` y `Reachable`. La adenda exige registrar y parar si eso ocurre; no se modifica la escena para pasarla.

## Hallazgo y TDD

Contacto `(.9,.8,8.6)`, humano en la esquina de atrás opuesta, `ClampBody((Width,Length))`, parámetros actuales de main:

| Dos paredes | ForecastBoast.Good | Humano Reachable |
|---|---|---|
| Low/Medium | true | true |
| Low/Strong | true | true |
| Mid/Medium | true | true |
| Mid/Strong | true | true |

Dirigidos: **21 tests, 15 Passed, 6 Failed, 0 Skipped**, XML fresco. B1 (adelante sin dos paredes) y B4 (guardias) pasan. B2 cumple su precondición pero elige 0 dos paredes: RED válido en Definidor y Completo. La prueba directa de la trayectoria falla por la intención directa anterior: RED válido. Los otros tres fallos son la precondición B3, **no** un RED de su propiedad de rechazo.

Se corrigió una preparación de mi test que descartaba faltas antes de comprobar B3; la segunda corrida, con las cuatro opciones, confirmó `Good=true` y `Reachable=true` en todas. Se mantuvieron contacto, posición, parámetros y criterio. Iteración propia autorizada por el encargo; ninguna modificación a tests existentes en main.

## Referencia main medida

Anexo A: 3 estilos × 3 niveles × 150 partidos, semillas 20261122. **42.986 filas**, idénticas byte a byte en dos corridas; 0 desalineaciones Decide/log y 0 SystemFault en las filas. R8 total: **1436/10632 = 13,5064 %**. Por estilo: Fondista 607/3921 = 15,4807 %, Definidor 414/3527 = 11,7380 %, Completo 415/3184 = 13,0339 %.

F1 original `BoastExperiment.M3ConfirmRound2`: 100 partidos por estilo/nivel, semillas 20270131. Medio: **Fondista 4,7195 %, Definidor 10,3464 %, Completo 8,8750 %**, cumple los criterios de M3 en main. Cero fallos de sistema en los nueve casilleros.

## Estado y límites

No se aplicó aún la política B: solo se añadió una extracción mecánica del solver de intención para probar la trayectoria real del filtro. B5, F1/F2 de la rama, PlayMode, AutoQA y check-release **no evaluados por la parada**. No hay resultados verdes de B ni comparación ganadoras rama/main. Defaults `207da8f2c636`, RallyParams `1063d0f6cf66` y SimulationKey `166a96e319c7`: guardia Passed, sin cambios.

[Protocolo, autorización y evidencia detallada](../validation/2026-11-23-rival-short-b.md). Logs y XML ignorados en `Logs/experiments/` y `Logs/evidence/rival-short-min/b-*/`. Cada corrida adquirió y liberó su candado. Sin cambios en física, solver compartido, presets, pesos, umbrales ni oráculos; sin fusión, build nativa, instalación o acceso al iPhone.
