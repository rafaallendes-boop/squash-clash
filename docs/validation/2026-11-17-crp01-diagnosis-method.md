# CRP-01 — método de diagnóstico (declarado antes de ejecutar)

Fecha: 17 nov 2026. Rama `claude/crp01-diagnosis` (= `main` `bddcef0`). Fase A: solo diagnóstico; no se modifica producto, el test existente, `DefaultPreset.json`, AutoQA ni ProjectSettings.

## Qué se sabe antes de correr

Test: `ReticleVisibilityTests.DevOpen_ProtocolActionsRemainUsable_AndCloseDevBeforeArming`. Por cada botón `s1-*`: `panel.Toggle()`, dos frames, `scroll.ScrollTo(button)`, dos frames, aserción de bounds. Falla con `button.worldBound.yMax` 788,198853 > `contentViewport.worldBound.yMax+1` 674,663208 (diferencia 113,5 px).

## Hipótesis (por lectura de código, aún no probadas)

- H1. `ShotOverlay.Update` refresca cada 0,1 s los labels `shot-preview-state/power/sampling/status`, que están **encima** de los botones `s1-*` dentro del `parameters-scroll`, con `whiteSpace=Normal`, y solo escribe texto si el panel está abierto. Tras `Toggle()` los labels pueden pasar de vacíos a multilínea después de que `ScrollTo` ya calculó el offset; el contenido crece y el botón baja. Que el 0,1 s caiga dentro o fuera de los dos frames depende de la velocidad de frames, lo que explicaría lo intermitente.
- H2. `DevPanelView.Update` refresca cada 0,5 s (`ShotSection`, `TaskSection`, parámetros), con el mismo efecto sobre la altura.
- H3. `ScrollTo` se evalúa antes de que el layout inicial del `ScrollView` esté resuelto (el panel pasa de `display:none` a flex en `Toggle`).
- H4. Estado dejado por un test anterior o resolución de Game view: se descarta primero con la corrida aislada; se revisa si solo aparece en la suite completa.

## Qué se ejecuta

1. **Línea base**: `scripts/test.sh playmode ReticleVisibilityTests`, **N = 10** corridas (Unity entero cada una), sin tocar nada. Un solo Unity a la vez; `git checkout -- ProjectSettings` tras cada una.
2. **Experimento instrumentado** (`ReticleVisibilityCrp01Experiment`, `[Explicit]`, `Category("Experiments")`, archivo nuevo en `Tests/PlayMode/`, con la misma preparación del test original copiada, sin editarlo): repite **25 ciclos** de los cuatro botones en una sola sesión de Unity y, por ciclo y botón, registra tras cada frame (0 a 5 y a +0,25 s): `button.worldBound`, `contentViewport.worldBound`, `contentContainer.layout`, `scrollOffset`, alto de los cuatro labels y tiempo real. Evalúa el mismo criterio que el original (sin lanzar fallo) y lo marca `WOULD_FAIL`. Se repite en **3 sesiones de Unity**. Una variante: mismas lecturas pero forzando el refresco de ShotOverlay (esperar >0,1 s antes de `ScrollTo`) para contrastar H1.
3. **PlayMode completa**: **una sola**, solo si 1 y 2 no reproducen, para ver dependencia del orden.

## Qué cuenta como reproducción

- Del test original: `Failed` con la aserción de la línea del `yMax` y valor observado > límite.
- Del experimento: algún `WOULD_FAIL` con `button.yMax − viewport.yMax > 1`. Se reporta tasa = ciclos con WOULD_FAIL / ciclos totales, por botón, y la serie de lecturas del primer caso, para atribuir la causa a un cambio de `contentContainer.layout.height` o de `scrollOffset` posterior a `ScrollTo`.
- Si nada reproduce, se informa como no reproducido (tasa 0/N), sin concluir que la causa sea el entorno.

## Evidencia

Logs/XML en `Logs/evidence/crp01/` (ignorada, no se versiona); resumen curado y hashes SHA-256 en el informe `docs/reviews/2026-11-17-crp01-diagnosis.md`.
