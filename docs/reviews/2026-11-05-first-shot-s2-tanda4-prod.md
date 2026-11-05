# First Shot S2 — renovación del perfil y tanda 4 PROD

## Alcance y perfil

El PO autorizó renovar el perfil conservando el equipo, bundle, dispositivo, entitlements, certificados y claves. Autorizó la instalación y el bloque 25 con su presencia, bajo la [predeclaración §§2, 5 y 8](../validation/2026-10-31-first-shot-s2-session27-predeclaration.md) y con la fecha límite operativa del **7 de noviembre de 2026 a las 17:58:46 UTC (14:58:46 Chile)**, incluida la restauración. Tanda 3 aprobada por Claude en `a0abbe3`.

Se conservó el perfil anterior (creado **2026-10-31T17:58:46Z**, vencimiento **2026-11-07T17:58:46Z**) como copia binaria del incorporado y se apartó de la caché solo el perfil de esta app. Xcode rechazó el primer intento de firma automática con huella explícita. Se reintentó con `Apple Development`, usando la identidad existente. [Procedimiento de Apple para solicitar un perfil nuevo](https://developer.apple.com/help/account/provisioning-profiles/edit-download-or-delete-profiles/).

El perfil nuevo incorporado en el `.app` se creó el **2026-11-05T13:55:07Z** y vence el **2026-11-12T13:55:07Z**. Identidad del perfil distinta, mismas listas de dispositivos y certificados, mismos entitlements y equipo/bundle cotejados. La firma usó el mismo certificado existente para la app y UnityFramework. Las identidades de firma instaladas permanecieron iguales. Verificación estricta del bundle inicial a las **2026-11-05T13:57:54Z**, repetida antes de instalar a las **2026-11-05T14:10:45.162016+00:00**. SHA-256 del perfil nuevo `0ff5c51cc830f535274e37a429351a6142afe2cde5b7c5a36b98d7ea68a76861`. Perfil anterior conservado y apartado, sin revocarlo. Sin exportar claves privadas, introducir contraseñas ni publicar el UDID.

## Build PROD firmada

**`0.1.0+8f030a8 prod`**, fuente de producto **`9131b1c`**. El nuevo gitHash identifica el head documental del export fresco. Sustituye a la build anterior `0.1.0+a11fb32 prod` para este recorrido. Cotejo Git vacío frente a `9131b1c` y SHA-256 iguales antes/después para **514 archivos** de Assets, ProjectSettings, Packages y scripts.

Check-release prod fresco con `gitDirty=false`, ejecutado desde una copia ignorada del script con los destinos de log/evidencia redirigidos y solo la iteración PROD. Core y Runtime incluidos, DevPanel y el framework de pruebas excluidos, puente diagnóstico de Runtime presente. Xcode Release en destino genérico, ambos binarios ARM64, IL2CPP Release, Unity no Development y sin `SQUASH_DEV`. SHA-256 del ejecutable `3b01b73e8fc7f332317af2589d5811a216bbe5b9d0d87f715fbc2d5e8a1f17ea`. Perfil anterior, controles y build nueva quedaron en rutas locales ignoradas. El worktree y ProjectSettings quedaron limpios.

## Instalación y precheck

El primer export DEV conservó la activación de «Legibilidad» registrada a las 13:53:40Z. El hash temporal pasó a `141920afa6af` con cero pendientes. Los tres archivos cotejados seguían intactos. Se conservó ese export. El primer reinicio DEV fue rechazado por bloqueo y el PO desbloqueó el iPhone.
Arranque DEV nuevo observado a las **2026-11-05T14:10:11.4607220Z**, con `0.1.0+2ef4807 dev`, P0 `dd122831f592` y cero pendientes. Respaldo nuevo `backup-before-install` cotejado con la referencia inicial y export DEV con el proceso nuevo `dev-p0-retry-export` terminado a las 2026-11-05T14:10:25.309518+00:00.
Instalación autorizada finalizada **2026-11-05T14:10:50.194649+00:00**. Antes de lanzar PROD, `post-install` cotejó los tres archivos por `cmp` y SHA-256 contra el respaldo nuevo y la referencia inicial a las 2026-11-05T14:10:51.444649+00:00.
Proceso PROD de comprobación `metrics-20261105T141054579-10b812cd3659429d9d13349052f65f82.jsonl` a las **2026-11-05T14:10:54.6003830Z**. Arranque `0.1.0+8f030a8 prod`, P0 `dd122831f592`, cero pendientes. Se verificaron el transitorio LaunchId 1 en L2 y el plan SL1 LaunchId 2 en L4. Se cerró este proceso de comprobación antes de pedir que el PO abriera el proceso medido. Sus lanzamientos quedaron fuera de cuota.
La lectura por USB de listado y copia individual se comprobó en DEV (0,62 s). En la primera ventana de vigilancia de 120 s no apareció un proceso nuevo. El PO indicó que el iPhone se había bloqueado por inactividad y lo desbloqueó. Se verificó que seguía sin proceso nuevo después del precheck y se volvió a activar la vigilancia. En esa primera ventana no se inició un bloque.

## Bloque 25 PROD

Proceso abierto por el PO `metrics-20261105T142547053-74b8b84609ad4b64b2ba48efb36caad7.jsonl` a las **2026-11-05T14:25:47.0661200Z**. El primer `shot-plan`, LaunchId 1 en L2, es el transitorio de First Ball. La frontera es el `shot-plan` SL1 de PracticeMode en L4, LaunchId 2, **2026-11-05T14:25:47.0732480Z**, `launchReal=2.1609576715351446`.
Se verificó en el Mac a las 2026-11-05T14:25:47.239706+00:00 `0.1.0+8f030a8 prod`, P0, `dd122831f592` y cero pendientes. Una segunda lectura por USB iniciada **después** de esa verificación (2026-11-05T14:25:47.239959+00:00) conservó el prefijo y aún terminaba en LaunchId 2. Así quedó cotejado el marcador antes de la primera recepción. La comprobación de orden usa los propios registros del proceso y no depende de sincronizar relojes.
Cuota **LaunchId 3–5**, tres recepciones iniciadas y completas. El LaunchId 6 en L26 confirma la frontera posterior. El PO avisó «termine 25» (registro Mac 2026-11-05T14:27:01.553125+00:00). Export de cierre terminado a las 2026-11-05T14:27:02.743224+00:00, SHA-256 del JSONL `c79c9c1414cd1c9897f2c58a454f59a7cf76d59ab4afe82fc758d7d0cd26291b`. Los tres LaunchId conservan el hash de recepción y, cuando existe gesto, también el hash de gesto. El precheck y su segunda copia son prefijos del original de cierre.

### Preguntas y respuestas literales

El PO contestó un campo por pregunta antes de mostrar resultados. Respuestas registradas a las 2026-11-05T14:31:39.692057+00:00.

| Pregunta | Respuesta del PO |
|---|---|
| Por número de recepción, ¿qué excepciones recuerdas: Apoyo sin querer, No fijó / falló u otra dificultad? | «todo ok» |
| ¿Viste el lanzamiento? | «si» |
| ¿Pudiste dar un golpe? | «si» |
| ¿Viste la nota del resultado? | «si» |
| ¿Oíste el sonido? | «tenía el cel en silencio a si que no» |

### Resultados observados

**3/3 recepciones completas**, 2 gestos en cuota, 2 Swing y 2 contactos aplicados. Finales: 1 NoOrder, 2 Perfect. 2 gestos extra fuera de cuota. El mapeo del gesto a recepción usa `launchSerial = LaunchId − 1`. Se conservaron las versiones originales y se resume la última por recepción.

| R | LaunchId | Línea de plan/final | Versión | Resultado / motivo | P (%) si Swing | Aplicado | Gesto registrado |
|---:|---:|---|---:|---|---:|---|---|
| 1 | 3 | 8/9 | 1 | NoOrder / reception-ended-without-order | — | No | — |
| 2 | 4 | 12/16 | 2 | Perfect / applied | 69.403172 | Sí | L13 `2b8b85995f5f4830a804ef44136b14ad` Swing |
| 3 | 5 | 19/23 | 2 | Perfect / applied | 61.204249 | Sí | L20 `f16f1f6084ae495484c8a933fa9f286d` Swing |

Extras: LaunchId 6 L27 AbandonedNoPull, LaunchId 6 L29 Swing.

| Métrica | n | p50 | p95 | p99 | Máximo |
|---|---:|---:|---:|---:|---:|
| Planificador (ms, uno por recepción) | 3 | 0.137500 | 0.158700 | 0.158700 | 0.158700 |
| Duración de Swing (ms) | 2 | 1154.359917 | 1521.146375 | 1521.146375 | 1521.146375 |
| Descenso al soltar (mm estimados) | 2 | 22.197403 | 24.903046 | 24.903046 | 24.903046 |
| Aim Began a C, referencia verbal sin marcador (m) | 2 | 0.981937 | 1.400226 | 1.400226 | 1.400226 |
| Δ de Swing (ms) | 2 | -38.355530 | -37.706363 | -37.706363 | -37.706363 |
| Salto (m) | 2 | 0.013442 | 0.016104 | 0.016104 | 0.016104 |
| Salto (px con HasJumpPixels) | 2 | 0.862545 | 1.033033 | 1.033033 | 1.033033 |
| Salto (diámetros) | 2 | 0.336058 | 0.402610 | 0.402610 | 0.402610 |
| Solver (ms) | 2 | 0.083300 | 0.087300 | 0.087300 | 0.087300 |
| Reconstrucción (ms) | 2 | 0.011400 | 0.016000 | 0.016000 | 0.016000 |
| Predicción (ms) | 2 | 0.056000 | 0.129100 | 0.129100 | 0.129100 |
| Reconciliación (ms) | 2 | 0.036200 | 2.130200 | 2.130200 | 2.130200 |

Percentiles crudos por rango más próximo. Los costes de contacto corresponden solo a golpes aplicados. Frame completo asociado al golpe y asignaciones por parte quedan NotEvaluated. El PO confirmó lanzamiento, golpe y nota del resultado. El log confirma dos contactos. El sonido queda **sin verificar** porque el PO indicó que el iPhone estaba en silencio. «todo ok» es una respuesta global y el feedback individual queda «sin respuesta». La recepción 1 quedó sin gesto registrado. Los dos gestos posteriores pertenecen a LaunchId 6, fuera de cuota. El recorrido PROD es funcional y se informa por separado.

## Restauración final y entrega

Tras el export se abrió el proceso nuevo `metrics-20261105T142709994-2de9475b88df459b8ed47d0a18397405.jsonl` a las **2026-11-05T14:27:10.0146510Z**, con `0.1.0+8f030a8 prod`, **P0-calib-1 `dd122831f592` y cero pendientes**. Export y cotejo final a las 2026-11-05T14:27:12.810360+00:00, antes del límite operativo y del vencimiento del perfil renovado. PROD queda instalada.

| Archivo | Bytes | SHA-256 final | Cotejo binario con respaldo nuevo e inicial |
|---|---:|---|---|
| `P0-calib-1.json` | 3614 | `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd` | Idéntico |
| `Po-1.json` | 2843 | `9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767` | Idéntico |
| `last-active.txt` | 10 | `5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087` | Idéntico |

Evidencia local ignorada en `Logs/prod-renewal-20261105/`. La entrega queda para revisión de Claude/PO. La sesión física queda detenida y PR #30, #31 y #32 siguen abiertas. S3–S4/T8 siguen sin autorización de implementación.
