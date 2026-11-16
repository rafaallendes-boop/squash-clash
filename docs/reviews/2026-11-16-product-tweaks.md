# C1 — Tres ajustes de producto (16 nov 2026)

**Implementado; detenido en PlayMode completo.** Rama `codex/product-tweaks` desde `main` `22bb2d5`. PR en borrador pendiente; sin fusión ni instalación.

**Decisión del PO:** «Ocultar línea M2-4 en prod», «Recordar la dificultad», «Diestro fija la derecha». Tras corregir la preparación de dos pruebas nuevas, autorizó registrar «dificultad inválida → Media» como guarda que ya pasa antes de implementar. **No se fabricó un RED.**

**Cambios.** `GesturePreview.Sling.cs` condiciona únicamente el `display` de `sling-status` por `SQUASH_DEV`: visible como antes en dev; `None`, sin espacio, en prod. El cálculo, las métricas y los registros conservan su ejecución. `FirstRunFlow` guarda la elección en PlayerPrefs (`sc.firstRun.level`) y la lee al inicializar; ausencia o entero inválido → Media. `CharacterView` fija la mano derecha del humano en el brazo −x (AX-1), simétrica a la izquierda +x, y cruza el brazo en el contacto opuesto; el rival conserva su comportamiento.

**TDD.** Se ampliaron `ZFirstRunMenuTests`, `ZHandednessTests` y su fixture compartida, conservando todas las pruebas y aserciones preexistentes. [Evidencia RED](../validation/2026-11-16-product-tweaks-red.json):

| Comprobación | Antes de implementar |
|---|---|
| Persistencia | RED por aserción: elegir Fácil no guarda 0; se obtiene −1. |
| Mano derecha fija | RED por aserción: tras golpes a ambos lados, se detecta cambio de mano. |
| Visibilidad sin `SQUASH_DEV` | RED por aserción: se esperaba `None`, se observó `Flex`. |
| Dificultad inválida | GREEN previo en −1, 3, mínimo y máximo entero; sin error de TearDown. |

La primera corrida (4 fallos) incluyó dos errores de preparación de Codex: exigir el texto «M2-4» y duplicar limpieza. Se registraron y se detuvo el trabajo. Con reanudación autorizada, se quitó esa exigencia y se limpió solo entre arranques, dejando el último al TearDown: segunda corrida **1 RED válido + 1 GREEN previo**, sin errores de preparación. Tras implementar, fixtures FirstRun/Handedness **27/27 GREEN**, salida 0.

**Compuertas** ([evidencia](../validation/2026-11-16-product-tweaks-gates.json)): EditMode completo **737/737**, salida 0; PlayMode completo **546/548**, 2 fallos, 0 omitidos, salida 1. `ProjectSettings` restaurado con `git checkout -- ProjectSettings`. AutoQA limpio PASS/0/nivel 1 y `check-release` dev/prod no ejecutados; PR pendiente. Un solo Unity a la vez; XML frescos y logs locales sin versionar.

**Detención.** Dos pruebas preexistentes de `ReticleVisibilityTests`, `SlingFeedbackAndStatus_StayOutsideFrontis_AndTargetMatchesScreenSide` (línea 160) y `M24FeedbackAndPlainCancelGuide_ClearProjectedFrontisAtNativeViewport` (línea 224), exigen `sling-status` visible (`Expected: True`, `But was: False`). La corrida no define `SQUASH_DEV`; ocultarlo es el nuevo comportamiento solicitado. Se registra el conflicto y se detiene conforme al encargo del PO, sin cambiar esas aserciones ni reintentar. No se declara GREEN completo ni se abre PR con compuertas incompletas.

**Límites y pendiente físico.** Sin cambios en Core, física, defaults, reloj, contrato, M2-4, gesto, potencia, `gesture.slingPreferLeft`, spec, escenarios, criterios ni presets. Se conservan pausa por interrupción y sonido/música independientes. En iPhone falta comprobar visibilidad dev/prod, dificultad tras cerrar y abrir y raqueta derecha al golpear por ambos lados; requiere autorización propia de instalación. La revisión independiente de Claude la coordina el PO; fusión y aceptación pendientes.
