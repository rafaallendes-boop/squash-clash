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
