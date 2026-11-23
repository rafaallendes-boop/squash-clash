# Anillos de la recepción del saque: ámbar y blanco, y «Espera el bote»

23 nov 2026. Rama `feat/reception-rings`, commit `1b71881` sobre `90ddc7e`. Encargo: `/Users/rafaallendes/sc-specs/impl-anillos.txt`, con la aprobación del PO del 23 nov, 16:33: «Sí, confirmo hasta 50 ms solo en mi recepción del saque y los anillos con «Espera el bote»». Implementado por Claude Sonnet con `/frontend-design` y TDD: **la revisión de Claude no es independiente** y queda para Codex. No fusionar.

**Estado: corregido el defecto propio; PlayMode 822/824 con 2 fallos previos a esta rama, sin atribuir a ella.** La PR no se fusiona (decisión de SC-PM/PO). Primera corrida sobre `1b71881`: PlayMode 807/824; la corrección del halo (`ca0e754`, autorizada por SC-PM) resolvió los 15 fallos propios.

## Qué cambia (solo cómo se ve y el texto)

- **Anillo ámbar (volea):** trazo del doble de grosor que el blanco (4 px frente a 2 px) y un halo suave, de alfa 0,30, en el mismo radio. El color `VolleyTint` no cambia.
- **Anillo blanco (bote):** igual que antes.
- **«Espera el bote»:** pastilla de cristal (Rubik Regular 14 px, estilo `.sc-ring-caption` de `Hud.uss`) sobre el círculo exterior del anillo blanco. Aparece solo en la recepción del saque del rival y mientras el anillo está visible; no aparece con el anillo ámbar, con la ayuda en nivel 2 ni en recepciones que no son saque. `RallyDirector` pasa al controlador un marcador de solo presentación (`SetNextReceptionServe`).
- No cambia cuándo aparece cada anillo, ni el contrato, el reloj, el solver, los presets ni los hashes de §5. No incluye los «hasta 50 ms»: esa medición e implementación son de la PR #106.

## Textos

| Texto | Estado | Veredicto del autor |
|---|---|---|
| «Espera el bote» | Nuevo | OK: «bote» es término de squash; español neutro, sin voseo. |
| Tutorial «Anillo ámbar visible: toca y voleas en el aire. Espera al anillo blanco: golpeas después del bote.» | Sin cambios | Sin cambios; el revisor lo puede reconsiderar. |
| Pista H1 «Anillo ámbar: toca y volea antes del bote.» | Sin cambios | Sin cambios. |

Sin dudas de vocabulario para SC-Lead. El revisor (Codex) debe incluir su sección «Textos».

## Pruebas

TDD con `scripts/test.sh`: RED por aserción (halo y trazo ausentes, `ServeReception` falso, vista del texto inexistente), luego GREEN `ZReception` 10/10. Todas las puertas con el lock de Unity.

| Puerta | `1b71881` (primera) | `ca0e754` (con la corrección del halo) |
|---|---|---|
| EditMode | 1000/1000 | 1000/1000 |
| PlayMode | 807/824, 17 fallos | **822/824, 2 fallos** |
| AutoQA `mvp` v / t3 / volea | PASS / 0 / nivel 1 (102/102) | PASS / 0 / nivel 1 (102/102) |
| check-release | OK (build 1151) | OK (build 1154, dev y prod) |

## Defecto propio, corregido

15 fallos de `TimingAidTests` (`FullAid_BoundariesMatchUnchangedContract`, 14 casos, y `AidPartial_HidesRingLast150ms`): comprueban que todos los `LineRenderer` del cue estén habilitados mientras el anillo es visible, y mi halo quedaba deshabilitado con el anillo blanco. Corrección en `ca0e754`: el halo se crea solo la primera vez que el anillo es ámbar (con `sortingOrder` −1), así un cue solo blanco conserva exactamente las líneas de siempre. No se modificó ninguna prueba existente, umbral ni criterio. Los 15 pasan.

## Los 2 fallos que quedan: previos a esta rama

`GesturePreviewTests.Main_WiresAimAndPowerWithoutChangingBallAuthority` (`DistinctSamples` 3, esperado ≥ 4) y `ReticleVisibilityTests.LocalCancelLimitAndState_AreVisibleAndNeverCoverFrontis` (esperado `True`, obtuvo `False`).

**Evidencia:** se ejecutaron solas en `90ddc7e` (la base de esta rama, sin ninguno de mis cambios), en la misma máquina: 2 ejecutadas, 0 aprobadas, 2 fallidas, con los mismos valores (3 frente a ≥ 4; `False` frente a `True`). Con la rama fallan igual. Por tanto no los causa este cambio. **No están en verde y no se dan por buenos:** la causa no se estableció (AGENTS §3 los cita como fallos de entorno solo en Linux; aquí es macOS, así que no se afirma que sean de entorno). Necesitan dueño y diagnóstico aparte.

Evidencia local, ignorada por git, en `Logs/evidence/rings-ca0e754/`: `playmode-after-fix.xml`, `editmode-results.xml`, `baseline-90ddc7e-two-tests.xml`; AutoQA en `Logs/autoqa/20261123T2338*-ca0e754-*`.

## Pendiente

- Diagnóstico y dueño de los 2 fallos previos de PlayMode (hoy el gate oficial de PlayMode no cierra en verde con o sin esta PR).
- Revisión independiente de Codex (incluida «Textos») y revisión visual en captura o iPhone; el aspecto todavía no se vio en pantalla.
