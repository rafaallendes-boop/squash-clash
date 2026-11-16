# U1 — detención tras PlayMode completo en el Mac

16 nov 2026 · Codex · PR #65 · código `3b14043` (implementación `b9ae0f6`).

**Estado: detenido, sin corrección ni reintento** conforme a AGENTS §2. No está listo para fusionar.

## Resultado nuevo

- EditMode completo: **738/738**, 0 Failed, 0 skipped, XML fresco y sin errores CS. Evidencia local `Logs/evidence/u1/editmode-green-*`.
- PlayMode completo: **557/558**, 1 Failed, 0 skipped, XML fresco y sin errores CS; **2026-11-16 21:42:42Z–21:48:03Z** (18:42:42–18:48:03 Chile). Salida 1.
- Fallo: `SquashClash.Runtime.Tests.ReticleVisibilityTests.DevOpen_ProtocolActionsRemainUsable_AndCloseDevBeforeArming`, `ReticleVisibilityTests.cs:213`. Después de ScrollTo y dos frames, el botón debe quedar dentro del viewport DEV. `button.worldBound.yMax` observado **788,198853**, límite `scroll.contentViewport.worldBound.yMax+1` **674,663208**. No se sabe qué iteración del bucle falló; no se inventa esa identificación.
- El test conserva los bytes de `a1fa915`; pasó en la corrida completa anterior (539/557). Esto no demuestra la causa ni permite atribuirlo al entorno o a intermitencia. Es el Mac oficial, no aplica la excepción Linux de AGENTS §3.
- Las **diez** pruebas AutoQAT3OracleTests pasan: seis casos de K/asistencia, SL1 +170 Late, K numérico desde posiciones/contactos, anclas V y guard de autoridad. Las 18 fallas previas por dependencia V/hash desaparecieron tras fijar V explícito y actualizar hashes autorizados, sin cambiar sus aserciones de comportamiento.

## Evidencia local ignorada

| Archivo en `Logs/evidence/u1/` | SHA-256 |
| --- | --- |
| defaults-red-retry-editmode-results.xml | `112ef7d88e208193a07d4459db4f268d0be19ae1341e8f276daa4226ce559305` |
| editmode-green-editmode-results.xml | `23edad3ea57b17694ad28e9ebd0f7b8142754e231ec973313809c37680a8d0b9` |
| playmode-after-fixture-pin-playmode-results.xml | `f0d4bb8d117344f04f4611b467e46d96c016350319adb4fabae091449ae9625a` |
| playmode-after-fixture-pin-playmode.log | `3f51f06c382aa8448c0d9b7c33f10ec4f4d4841e1a3ee810f08ab8c8134f206a` |

Hashes de defaults/P0/SL5 T3 medidos en Unity antes de AutoQA: `430da2faa818` / `dcc7c8cc0688` / `e89daab41c3e`. SimulationKey y proyecciones heredadas conservadas. Las anclas V explícitas permanecen `06fed4bd4ac9` / `8e22575422f2` según test. El archivo P0-calib-1 no se toca.

## Alcance conservado al detenerse

El catálogo V `mvp.json`, Core, solver, contrato, reloj y el test fallido no difieren de `a1fa915`. Único cambio de producto: `DefaultPreset.json` validLimit de 1 a 0. AutoQA V/T3 aún no ejecutado; no existe PASS de juego para esta entrega. Check-release, build nativa, instalación, acceso al iPhone y fusión no realizados. ProjectSettings restaurado con `git checkout -- ProjectSettings` después de salir completamente Unity; verificación final sin procesos de editor ni auxiliares. U3 y U2 no se inician por esta parada. Revisión independiente de Claude pendiente, coordinada por el PO.

## Decisión posterior del PO

El PO identifica la falla como recurrencia de **CRP-01** (12 nov; mismas cifras y test sin cambios) y la trata como **intermitente ajena a U1, sin causa demostrada y sin corregirla en #65**. Referencia histórica: `docs/reviews/2026-11-12-clockmap-rollback-precision.md`. Autoriza una nueva PlayMode completa; si pasa con el recuento completo, seguir AutoQA V/T3 (PASS/0/nivel 1 cada uno) y check-release. Si reaparece CRP-01 u otra falla, detenerse. Se conserva íntegra la evidencia anterior; no se reclasifica su 557/558 como PASS. Literal en el registro de aprobaciones.

## Cierre de la continuación autorizada

PlayMode nueva sobre `98a73ba`: **558/558**, 0 Failed/0 skipped, XML fresco, sin errores CS, 2026-11-16 22:47:58Z–22:50:09Z. CRP-01 no reapareció; el fallo anterior y su clasificación permanecen. Se esperó la terminación completa del exec y se restauró ProjectSettings antes de AutoQA. XML conservado en `Logs/evidence/u1/playmode-authorized-green-playmode-results.xml`, SHA-256 `8e9e1e3018ac56bed4ffb5f46f63ed8391101f01307c2f83be79129ff2ae465e`.

AutoQA V `20261116T225123Z-98a73ba-v-6765` y T3 `20261116T225257Z-98a73ba-t3-17437`: **PASS/0/nivel 1 cada uno**, 18 intentos PASS, canario FAIL, arnés 88/88 y todos los flags de integridad true; restauración conservó el estado del árbol. V conserva hashes efectivos originales. T3 observó todos los resultados predeclarados, incluido SL1 −200 Valid y SL5 +170 Valid. Ambas corridas con `dirty=false`.

Check-release dev/prod: OK, exportaciones frescas de `98a73ba`, `gitDirty=false` en ambos perfiles; DevPanel solo dev, puentes nativos/hápticos esperados presentes. Solo exportación Unity; sin compilación nativa, firma, instalación ni acceso al iPhone. Resumen curado de contadores, hashes y procedencia en `docs/validation/evidence/2026-11-16-u1-summary.json`; los logs/XML permanecen ignorados. Revisión independiente y aprobación de fusión pendientes.

## Integración de main tras #64

Por encargo del PO, merge de `origin/main` `10bcaec` en `b3c2fad`, con ambos bloques de aprobaciones íntegros. Único delta C# respecto al head previo: importación del experimento Session45HashExperiment de #64, sin ejecutarlo ni alterarlo. Código de producto y tests de regresión de U1 sin modificaciones. Nueva EditMode completa **738/738**, 0 Failed/0 skipped, XML fresco y sin errores CS. XML ignorado `Logs/evidence/u1/post-main-64-editmode-results.xml`, SHA-256 `9d5ac23ab30357c4653527fb8e201a1a486e1c71a1a2dbc612a39f09baf4e77f`. Las cifras PlayMode/AutoQA/release anteriores conservan sus heads y no se presentan como corridas nuevas. Fusión de #65 pendiente del PO.
