# HUD de DEV sin líneas técnicas y tutorial de dos paredes

21 nov 2026 · Rama `claude/store-prep-2`, base `main` `5a1d98d`. **Trabajo dirigido por Claude (subagente Sonnet); la revisión no es independiente.** Sin push, fusión ni instalación. Sin cambios en física, timing, contrato, `DefaultPreset.json` ni gameplay.

## 1. DEV: nada técnico abajo mientras se juega
- Se ocultan con `visibility: hidden` (no cambia el layout) la línea `P0-calib-1 · hash · Repetición activa` (`shot-overlay`, `ShotOverlay.cs`) y la línea `M2-4 · …` (`sling-status`, `GesturePreview.Sling.cs`).
- Visibles solo con el panel DEV abierto (o la barra visible por un experimento/sesión H17) o mientras corre un protocolo sling (resultado pendiente, bloque en curso, estado o aviso de rechazo). Misma regla que la barra de #85.
- **Banda inferior:** no existe. La barra es absoluta (overlay), no reserva espacio de layout ni recorta la cancha; oculta no deja banda. Nada que colapsar.
- Prod: sin cambios (sin módulo DevPanel; `sling-status` sigue en `display: none`).
- Tests existentes ajustados por contradecir la nueva regla con el panel cerrado: `ReticleVisibilityTests` (2 aserciones: ya no hay elementos del overlay visibles ni una línea compacta), `ZFirstRunMenuTests` (`sling-status` ya no `Shown` en DEV cerrado). El resto de aserciones y la geometría CRP-01 no cambian.

## 2. Onboarding: golpe de dos paredes
- Quinto paso «Dos paredes» (`FirstRunFlow.cs`, `TutorialSteps` 4 → 5), con dibujo propio (`DrawBoast`): «En el rally, toca la pared lateral que ves: la pelota pega primero en ella y después en el frente. Si tocas cerca de donde vas a pegar, cruza al otro lado. No vale en el saque.»
- **Usuarios existentes:** la marca `sc.firstRun.tutorialSeen` es booleana, no hay mecanismo de pasos nuevos; quien ya lo terminó no lo ve de nuevo a la fuerza, pero lo encuentra en «Cómo jugar» del menú. Quien no lo había visto, lo ve completo (5 pasos).
- Tests ajustados al nuevo conteo: `ZFirstRunTutorialTests`, `ZUserMetricsFlowTests` (4 → 5 pasos).

## RED / GREEN
- Líneas ocultas: `Closed_TechnicalLinesAreHidden_AndOpenShowsThem` RED por aserción («Expected: Hidden») sin el código; GREEN con él (la primera versión usaba `ProtectSlingProtocol`, siempre verdadero con `gesture.control` por defecto; se corrigió a las señales reales del protocolo).
- Tutorial: RED «Expected "1 de 5" but was "1 de 4"»; GREEN tras implementar.

## Compuertas (head `77d3521` más este documento)
EditMode 961/961, PlayMode 744/744, AutoQA V, T3 y volley PASS/0/nivel 1 (`…-77d3521-v-26530`, `…-t3-15028`, `…-volley-9035`), check-release dev/prod OK. Una corrida previa dio PlayMode 742/744 (metrics del tutorial, corregido) y un ERROR de T3 por haber editado un test durante la corrida; repetido limpio.

## Límites
Sin verificación visual en iPhone. El dibujo del paso 5 no se vio en pantalla (solo pasa los tests de ajuste y legibilidad).
