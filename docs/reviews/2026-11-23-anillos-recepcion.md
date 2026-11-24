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

## Diagnóstico de los 2 fallos previos (encargo de SC-PM, solo diagnóstico)

**Conclusión: no es del código del producto ni de esta rama. Es el DPI que reporta este Mac sin pantalla, al que las dos pruebas están acopladas.** Se ejecutaron en `90ddc7e` y en la rama (`b5968e4`) con resultados idénticos (12 pruebas: 4 aprobadas, 8 falladas, en ambas).

**Evidencia:**

1. Unity reporta `Screen.dpi = 72` en esta ejecución (`DIAG screen.dpi=72`). `ScreenMetrics.MmPerPx` solo acepta un DPI reportado entre 150 y 700; si no, usa `input.fallbackDpi`, cuyo valor por defecto aquí resulta 460 (la ejecución sin override se comporta igual que la de 460). Además, `system_profiler SPDisplaysDataType` no lista ninguna pantalla conectada a este Mac (solo el chip): no hay densidad real que reportar.
2. `GesturePreviewTests.Main_WiresAimAndPower…`: copia diagnóstica con `input.fallbackDpi` distinto (resto idéntico). Con 160, 254 y 326: `DistinctSamples` 4, pasa. Con 460 (y sin override): 3 (`few-samples`), falla. A 460 mm/px el primer movimiento de 40 px queda dentro del radio de toque y el arrastre empieza una muestra más tarde.
3. `ReticleVisibilityTests.LocalCancelLimit…`: usa `MmPerPx(Screen.dpi, 460)` fijo. Copia diagnóstica con DPI constante: pasa con 254; falla con 0 (= real), 160, 326 y 460. Es decir, el diseño de la prueba solo se cumple en una ventana estrecha de densidades.
4. El mismo comportamiento en `90ddc7e` y en la rama descarta una regresión de esta PR.

**Atribución y límites:** es del entorno de este Mac en esta ejecución (sin pantalla, `Screen.dpi` 72) **y** de la prueba, que depende de `Screen.dpi` en vez de fijarlo. No se verificó que el Mac del PO con pantalla reporte ~254; es la inferencia por la que ahí pasan (AGENTS §6: PlayMode 746/746). Las copias diagnósticas eran temporales, no se commitearon y no se modificó ninguna prueba original, umbral ni criterio.

**Propuesta de arreglo (sin aplicar, requiere dueño):**
- Corto plazo: correr el gate oficial de PlayMode con la pantalla conectada y comprobar `Screen.dpi` en el log antes de dar un resultado por válido.
- De fondo: hacer las dos pruebas herméticas. Un gancho de prueba para el DPI reportado en `GesturePreview` (como `InputAreaOverride`) y que ambas pruebas lo fijen (254), en lugar de leer `Screen.dpi`. La segunda debería además revisarse por qué solo cumple en torno a 254. Cambia pruebas protegidas y un gancho en Runtime: decisión de SC-PM/PO.

Evidencia local, ignorada: `Logs/evidence/dpi-diagnosis/{branch,main}.xml`.

## Pendiente

- Decisión sobre el arreglo de los 2 fallos previos de PlayMode (ver diagnóstico) y volver a correr el gate con pantalla conectada.
- Revisión independiente de Codex (incluida «Textos») y revisión visual en captura o iPhone; el aspecto todavía no se vio en pantalla.
