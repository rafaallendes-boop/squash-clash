# C1 — Tres ajustes de producto (16 nov 2026)

**Implementado y compuertas GREEN.** `codex/product-tweaks` desde `main` `22bb2d5`; entrega en borrador, sin fusión ni aceptación física.

**Cambios.** `sling-status` conserva su cálculo y registros; solo su `display` depende de `SQUASH_DEV`: comportamiento anterior en dev, oculto sin espacio en prod. Se eligió el cambio mínimo en `GesturePreview.Sling.cs`. `FirstRunFlow` guarda dificultad en PlayerPrefs (`sc.firstRun.level`) y la restaura al inicializar; ausencia o entero inválido → Media. `CharacterView` fija la derecha del humano en −x (AX-1), simétrica a la izquierda +x, y cruza el brazo en contactos opuestos. El rival conserva su comportamiento.

**TDD** ([RED](../validation/2026-11-16-product-tweaks-red.json), [compuertas](../validation/2026-11-16-product-tweaks-gates.json)):

| Comprobación | Antes de implementar |
|---|---|
| Persistencia | RED por aserción: elegir Fácil devuelve −1 en vez del 0 guardado esperado. |
| Derecha fija | RED por aserción: se detecta cambio de mano tras golpes a ambos lados. |
| Visibilidad sin define | RED por aserción: `Flex` en vez de `None`. |
| Dificultad inválida | Guarda GREEN previa en −1, 3, mínimo/máximo entero, sin error de TearDown. |

El PO autorizó registrar esa guarda sin fabricar RED. Dos errores iniciales de preparación (texto «M2-4» y limpieza duplicada) se registraron y corrigieron con autorización. Fixtures FirstRun/Handedness: **27/27 GREEN**. El primer PlayMode completo dio **546/548**: dos pruebas existentes exigían la línea visible sin define. Se detuvo y registró; el PO autorizó esta excepción literal:

> Autorizo modificar las dos pruebas existentes de `ReticleVisibilityTests` (`SlingFeedbackAndStatus_StayOutsideFrontis_AndTargetMatchesScreenSide` y `M24FeedbackAndPlainCancelGuide_ClearProjectedFrontisAtNativeViewport`), solo en lo que toca a `sling-status`. Con `SQUASH_DEV` se conservan las aserciones actuales: visible, sin tapar el frontis y sobre la barra. Sin el define, se exige que esté oculto y se omiten sus aserciones de posición. No cambies nada de los demás elementos ni de otras pruebas.

Se modificó únicamente ese caso en ambas pruebas. Los demás elementos y pruebas preexistentes conservan sus aserciones; también se comprobó igualdad del código de las aserciones al preprocesar para dev.

**Compuertas finales:** EditMode **737/737**; PlayMode **548/548**; ambos salida 0, sin omitidos. Tras PlayMode, `git checkout -- ProjectSettings`. AutoQA **PASS / 0 / nivel 1 / dirty=false**, 85/85 pruebas, 18 intentos PASS y canario FAIL esperado; restauración correcta. `check-release`: **OK dev/prod**, exportaciones nuevas y limpias. El C++ generado conserva la asignación condicional en dev y `None` constante en prod. AutoQA/release sobre `4efed34`; después solo informe/evidencia. Un solo Unity, XML frescos; logs y builds sin versionar.

**Pendiente en iPhone:** comprobar visibilidad dev/prod, dificultad tras cerrar/abrir y mano derecha en ambos lados. Solo exportación Unity; sin compilación nativa, instalación ni acceso al teléfono. Core, física, defaults, reloj, contrato, gesto, potencia, `gesture.slingPreferLeft`, spec, escenarios, criterios y presets intactos. Pausa por interrupción y sonido/música independientes conservados. Revisión independiente de Claude coordinada por el PO y aprobación de fusión pendientes.
