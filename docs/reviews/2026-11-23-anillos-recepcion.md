# Anillos de la recepción del saque: ámbar y blanco, y «Espera el bote»

23 nov 2026. Rama `feat/reception-rings`, commit `1b71881` sobre `90ddc7e`. Encargo: `/Users/rafaallendes/sc-specs/impl-anillos.txt`, con la aprobación del PO del 23 nov, 16:33: «Sí, confirmo hasta 50 ms solo en mi recepción del saque y los anillos con «Espera el bote»». Implementado por Claude Sonnet con `/frontend-design` y TDD: **la revisión de Claude no es independiente** y queda para Codex. No fusionar.

**Estado: detenido por defecto en PlayMode.** Se aplica AGENTS.md §2 («registrar y detenerse, sin corregir ni reintentar para obtener Pass»). La PR no está lista para fusionar.

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

TDD con `scripts/test.sh`: RED por aserción (halo y trazo ausentes, `ServeReception` falso, vista del texto inexistente), luego GREEN `ZReception` 10/10. Sobre `1b71881`, con el lock de Unity:

| Puerta | Resultado |
|---|---|
| EditMode | 1000/1000 |
| PlayMode | **807/824, 17 fallos** |
| AutoQA `mvp` v / t3 / volea | PASS / 0 / nivel 1 en las tres (102/102) |
| check-release | OK (build 1151, dev y prod) |

## Defecto: 17 fallos de PlayMode

1. **14 fallos de `TimingAidTests` (causa identificada, es mío).** `FullAid_BoundariesMatchUnchangedContract` (13 casos) y `AidPartial_HidesRingLast150ms` comprueban que *todos* los `LineRenderer` del cue estén habilitados mientras el anillo es visible. El halo nuevo («Volley halo») está deshabilitado con el anillo blanco y rompe esa invariante. No se modificó la prueba. Corrección propuesta, sin aplicar: crear el halo solo cuando el anillo es ámbar por primera vez, de modo que un anillo solo blanco siga teniendo únicamente líneas habilitadas.
2. **3 fallos sin atribuir:** `GesturePreviewTests.Main_WiresAimAndPowerWithoutChangingBallAuthority` (`DistinctSamples` 3, esperado ≥ 4), `ReticleVisibilityTests.LocalCancelLimitAndState_AreVisibleAndNeverCoverFrontis` y una tercera de `TimingAidTests` ya contada arriba. Las dos primeras no tocan los anillos; AGENTS §3 las cita como posibles fallos de entorno. No se midió una línea base en `main`, así que no se afirma que sean previas.

No se reintentó ni se corrigió después de detectarlo. Evidencia local, ignorada por git: `Logs/playmode-results.xml`, `Logs/playmode.log`, `Logs/autoqa/20261123T2216*-1b71881-*`.

## Pendiente

- Decisión de SC-PM/PO: aplicar la corrección propuesta del halo y repetir PlayMode, y medir una línea base de los dos fallos sin atribuir en `main`.
- Revisión independiente de Codex (incluida «Textos») y revisión visual en captura o iPhone; el aspecto todavía no se vio en pantalla.
