# Saque nuevo encendido por defecto (RAL-177) · 22 nov 2026

Segundo incremento de RAL-177. La rama incorporó `origin/main` `d26de5f` mediante el merge `6b54a95`. «Saque nuevo» arranca activado y el panel DEV permite volver al saque original.

La primera corrida con el saque activo dio 964/965 en EditMode por un fallo de sistema en `VolleyV21Tests`. Claude identificó un contacto que P2 colocó en la pared izquierda y que el solver rechazó al comparar con el límite a mayor precisión. [Diagnóstico en #99](https://github.com/rafaallendes-boop/squash-clash/pull/99#issuecomment-7088576915).

El PO autorizó el 22 nov a las 23:03 −03 en el chat de SC-PM:

> «Sí, autorizo corregir la precisión del borde en ShotSolver para el saque nuevo»

`ShotSolver` usa `(float)(court.Width-radius)` y `(float)(court.Length-radius)` en los máximos de pared. La corrección mantiene los parámetros y hashes.

El PO aceptó la desviación sin recalibrar. El Definidor baja −5,1 pp en Medio y −5,5 pp en Difícil. La devolución del saque del rival por el novato pasa de 97,5 % a 89,7 % en P+V. Queda abierta su revisión con los primeros usuarios reales de TestFlight. [Registro con las dos citas](2026-11-18-po-approvals.md#22-nov-2026--saque-nuevo-encendido-por-defecto-ral-177).

## TDD y aserciones

- `ShotSolverTests.ContactAtP2LeftWallLimit_ReachesTheFrontWall`: RED 0/1 por `target-or-contact-outside-court`. El test comprueba que el golpe resuelve y su primer choque en P2 es con el frontis. GREEN del caso nuevo y de `VolleyV21Tests` en la suite completa. `VolleyV21Tests` se conserva intacto.
- `ServeTests.NewServeIsEnabledByDefault`: RED 0/1 por aserción, esperaba 1 y obtuvo 0. La prueba `Off_EveryKindIsTodaysAutomaticServe_AndNothingIsChosen` verifica ahora el apagado explícito y conserva la identidad del saque original.
- `ZServeTests.Off_TheMatchServeIsTodaysAutomaticServe`: apagado explícito para conservar las aserciones del saque original. `DevPanel_NewServeToggle_DefaultsOn_AndSwitchesTheNextPoint`: inicia en «sí», pasa a «no» y vuelve a «sí».
- `ZFirstRunTutorialTests.FirstPlay`: el contador llega a 6 de 6, «Saque» es el sexto paso y «¡A jugar!» aparece al final. `HowToPlay` conserva el recorrido de cinco pasos con el interruptor apagado.
- `ZUserMetricsFlowTests`: el recorrido completo y el salto registran seis pasos. `EveryStep_FitsThePortraitScreen`, `Tutorial_EveryStepFitsInsideTheSafeArea` y `PlayerText_IsReadable` recorren los seis.
- `Match_HeaderBannerPauseAndResultFitInsideTheSafeArea`, `Interruption_BetweenPoints_KeepsTheScoreAndTheBanner` e `InAMatch_TheBannerSaysWhoServesNext_AndTheScorePulses` fijan el apagado para conservar las aserciones sobre el banner ordinario.
- Los replays congelados de `FatigueIntegrationTests`, `FatigueArmRallyTests` y `RivalAiTests` fijan el saque original que usaron sus referencias. Se conservan las firmas y las proyecciones históricas.

## Compuertas en el Mac del PO, sobre `c468ef18f8d0a09d00c50589daede6abc3abb308`

- EditMode completo **968/968** y PlayMode completo **762/762**, cero fallos y cero omitidos.
- AutoQA V, T3 y volea **PASS / salida 0 / nivel 1**, 102/102 por suite.
- `check-release` dev/prod **OK**, build 1113.

`RallyParams` pasa de `32683515d986` a `1063d0f6cf66`. La proyección sin `ServeKind*` sigue en `b2311f168c30`. Defaults T3 `207da8f2c636` y SimulationKey `166a96e319c7`. La confirmación por estilo de la spec §7 queda para después de la fusión para dar prioridad a la build. Revisión de Claude pendiente.
