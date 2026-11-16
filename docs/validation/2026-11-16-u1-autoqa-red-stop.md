# U1 — evidencia de RED y detención operativa

16 nov 2026 · Codex · PR #65. La predeclaración V/T3 está en `df37652`, anterior a cualquier corrida de AutoQA (ninguna realizada). No se cambian criterios ni escenarios V.

## Corridas

1. `scripts/test.sh playmode AutoQAT3OracleTests`: XML fresco, total 9, passed 5, failed 4, skipped 0. Tres RED válidos por aserción: `T3_SL1Late170_RemainsLateOutsideK` y `T3_UsesKBeforeAssistance` a −320/+320 ms esperan Failed y reciben Valid. El cuarto fallo fue una fixture nueva inválida (`human.x=0`, fuera de rango); no cuenta como RED. Se corrigió la traslación a la posición corporal permitida, sin cambiar la expectativa.
2. `scripts/test.sh playmode K_IsMeasuredFromRawPositionsAndCollisionHistory`: XML fresco, total 1, passed 0, failed 1, skipped 0. RED válido: K-in esperado −12,820512820512821 ms, observado −Infinity del esqueleto. Los scripts/test.sh de ambas corridas terminaron con salida 1. Sin errores CS. `git checkout -- ProjectSettings` ejecutado tras PlayMode.
3. `scripts/test.sh editmode Defaults_SelectT3Pure_WithoutSoftEarlyOrLatencyCompensation`: Codex lo lanzó mientras el exec de la corrida anterior aún devolvía una sesión activa, durante el cierre de Unity. El motor rechazó abrir el proyecto por instancia existente. Salida 1, total aceptado 0; el XML disponible era el del test anterior de seis recetas del turno previo, **no posterior a esta corrida**. Resultado **NotEvaluated**, no RED ni PASS. No se reintentó.
4. `python3 scripts/tests/test_autoqa_wrapper.py`: 18/18, 35,435 s, salida 0; fixtures de proceso independientes, sin Unity. Esta corrida ya estaba abierta y se recogió su final tras la detención. No acredita AutoQA de juego.

## Evidencia local ignorada

Conservada en `Logs/evidence/u1/`, con manifest local SHA-256. No se versionan logs/XML.

| Archivo | SHA-256 |
| --- | --- |
| oracle-red-playmode-results.xml | `cdd21e795f81547dfbe9cf22ca259b24bbacb7714f697e218110fe102b1f0043` |
| oracle-red-playmode.log | `12eab1c43507befbbc771db79d8a2d3571418090a6e6fbe00b18c4c5a0779a62` |
| k-red-playmode-results.xml | `7b745c8cd76bc0e179ec398328da1b185c8f1f650dad130c7257c2317dd74f47` |
| k-red-playmode.log | `c7d299483144b104c2af3f5b63ba79b64e61cad1ed3a93157b1229c27239b7a9` |
| defaults-blocked-editmode-console.log | `ba8ee87264ea0d60d4bf9e11554ae181715bbc353d5b8134eab16ab4bab8c271` |
| defaults-blocked-editmode.log | `23a7ca9478e3604aeeb108e6d6e03fcb6b6b959a311d6afdf2060e6264711dd4` |
| defaults-blocked-editmode-results.xml (viejo; rechazado) | `949fd883db8fb4fded9072bf6d68deddfcca55f040765e41761eb9c05561d97e` |

## Estado al detenerse

Error de secuencia del operador Codex, no defecto del producto ni del contrato. No se atribuye al entorno y no se oculta con reintentos. No hay Unity activo al entregar. El cálculo independiente de K y el oráculo siguen como esqueletos compilables con tests RED; `DefaultPreset.json`, presets, física, solver, contrato y reloj no se tocaron. La ampliación de escenarios aún no se implementó. Sin AutoQA V/T3, suites completas, medición de nuevos hashes, check-release, build, instalación o fusión. U3/U2 siguen pendientes. Cualquier reanudación debe esperar el fin del proceso completo, no solo la escritura del XML, antes de abrir otro Unity.
