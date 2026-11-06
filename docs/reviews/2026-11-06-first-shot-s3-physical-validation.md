# First Shot S3 — validación física, sesión detenida tras S3-A

**6 de noviembre de 2026, Chile.** Operación de Claude conforme a la [autorización del PO](2026-10-25-first-shot-po-approvals.md#autorización-de-la-validación-física-de-s3--6-de-noviembre-de-2026-chile) y la [predeclaración](../validation/2026-11-06-first-shot-s3-physical-predeclaration.md), confirmada en `fab60ef` antes del primer bloque. **Sesión detenida después de S3-A** por ausencia de sonido. S3-A se conserva como **evidencia parcial, no válida ni completa**, por [decisión del PO](2026-10-25-first-shot-po-approvals.md#detención-de-s3-y-corrección-puntual-del-audio--6-de-noviembre-de-2026-chile). S3-B a S3-E y la observación de pausa no se ejecutaron.

Horas en UTC; Chile es UTC−03:00. Evidencia local ignorada en `Logs/evidence/s3-physical-20261106/`, con el registro de horas Mac y respuestas en `po-log.txt`.

## 1. Preparación

- **Fuente y build.** `fab60ef`: delta frente a `main` `adb8c8f` solo en `AGENTS.md` y dos documentos. `check-release` dev y prod **OK** con `gitDirty=false`; DevPanel solo en dev; puente háptico y símbolos en ambos. Build **`0.1.0+fab60ef dev`**: Release, IL2CPP, arm64, `development=False`, `SQUASH_DEV`. Firma estricta correcta. SHA-256 de `UnityFramework` `7de61d5b60fb513c523ddc8d9f8658a34f7f59132a836ad432a6e15dc673bf82`. Suites no repetidas: el producto es idéntico al de la regresión 546/546 y 353/353 sobre `067ca91`.
- **Perfil incorporado.** Creado 2026-11-05T13:55:07Z, vence **2026-11-12T13:55:07Z**; equipo, bundle y dispositivo correctos; SHA-256 `0ff5c51cc830f535274e37a429351a6142afe2cde5b7c5a36b98d7ea68a76861`, el mismo perfil renovado de tanda 4. Comprobado a las 11:47:05Z, con 146 h de margen.
- **Hashes esperados.** Reproducción de `ParamSet.ComputeHash` sobre el manifiesto B de S2 (que da `dd122831f592`) más `feedback.haptics`: **`6ecfdfa78cbb`** para P0 y `4f33dff747fb` para S3-E. El primero coincide con el declarado en la implementación.
- **Respaldo.** Export de la app PROD `0.1.0+8f030a8` a las 11:47:21Z. `P0-calib-1.json` (3614 bytes), `Po-1.json` (2843) y `last-active.txt` (10, `P0-calib-1`) **idénticos a la referencia inicial** por SHA-256.
- **Instalación.** Dev instalada sobre PROD a las 11:47:43Z, sin desinstalar. Los tres archivos, idénticos al respaldo antes de abrir la app.
- **Arranque.** Proceso nuevo 11:48:00.1766540Z: `0.1.0+fab60ef dev`, `P0-calib-1`, `presetFileHash` `2f1295f2…`, **`effectiveHash` `6ecfdfa78cbb`**, `pending=0`. Los tres archivos, idénticos también después del arranque.
- **Incidencia previa, solo registro.** El proceso PROD de restauración de ayer (`metrics-20261105T142709994…`) siguió en uso después de cerrar la tanda 4: 35 lanzamientos y 21 gestos hasta las 2026-11-05T20:38:21Z, todos con `dd122831f592`. Quedan fuera de toda tanda y sin atribución.

## 2. Decisiones del PO durante la preparación

Respuestas literales, con la hora Mac de registro:

- 11:48:53Z, observación de pausa fuera de cuota después de S3-E: «Sí, incluirla».
- 11:50:36Z: «Si, usaré mano izquierda. Con respecto al iphone, lo dejé en duplicar y con el timbre activado».
- Claude advirtió que con Duplicación el sonido sale por el Mac y la vibración no llega al iPhone, y pidió jugar con el iPhone en la mano y confirmar volumen audible y vibración del sistema. Respuesta, 11:52:42Z: «Si, probaré desde el iphone y te confirmo que está el volumen y vibración lista».

## 3. S3-A · Contacto — evidencia parcial

**Precheck aceptado** a las 11:53:05Z, sobre el proceso nuevo `metrics-20261106T115246125-a6248a6719b64a77a3420ffaacf27fdb.jsonl` (11:52:46Z): build, P0, `6ecfdfa78cbb`, `pending=0`, lanzamientos avanzando sin pausa ni overlay y los tres archivos idénticos al respaldo.

**Desviación: sin frontera DEV.** El registro no contiene ninguna línea `dev-control`. El PO respondió «No, empecé directo». Sin la línea de cierre de DEV no se puede aplicar la regla de cuota predeclarada. Las recepciones siguientes se describen como observadas, no como cuota.

**Recepciones observadas.** Tras el `NoOrder` de LaunchId 99, el PO hizo un gesto por recepción en ocho LaunchId consecutivos (100–107), entre las 11:59:40Z y las 12:00:17Z. El PO declaró «hice mas de 6». Todas con hash de lanzamiento y de gesto `6ecfdfa78cbb`, versión final 2.

| LaunchId | Gesto | Resultado final | Motivo | Aplicado | P | Δ (ms) | Presentación (categoría / contacto / fallo) |
|---:|---|---|---|---|---:|---:|---|
| 99 | — | NoOrder | reception-ended-without-order | No | — | — | — |
| 100 | Swing | Perfect | applied | Sí | 0,522 | −18,6 | Perfect / sí / — |
| 101 | Swing | Failed | outside-v | No | 0,366 | −151,3 | Failed / no / Air |
| 102 | Swing | Valid | applied | Sí | 0,510 | −93,8 | Valid / sí / — |
| 103 | Swing | Failed | outside-v | No | 0,507 | −102,7 | Failed / no / Air |
| 104 | Swing | Failed | outside-v | No | 0,562 | −202,1 | Failed / no / Air |
| 105 | Swing | Valid | applied | Sí | 0,589 | −84,8 | Valid / sí / — |
| 106 | Swing | Valid | applied | Sí | 0,552 | −77,4 | Valid / sí / — |
| 107 | Swing | Valid | applied | Sí | 0,415 | −52,4 | Valid / sí / — |

**Comprobaciones del registro (§7 de la predeclaración):** 8 presentaciones, una por intento; las 5 de contacto (1 Perfect, 4 Valid) con contacto aplicado; las 3 Failed con resultado final Failed y fallo `Air`; ninguna presentación para `NoOrder`; **cero** `shot-presentation-discrepancy`. El registro acredita las peticiones de presentación, no que se oyeran o sintieran.

**Fuera del bloque.** Tras «Listo termine S3-A» (12:00:47Z), la app no lanzó entre 12:00:15Z y 12:15:39Z, compatible con el iPhone bloqueado. A las 12:15:38Z hubo toques filtrados por la interfaz y un gesto extra en LaunchId 108 (Perfect aplicado), fuera de cuota. A las 11:59:32Z, antes del primer gesto, hubo un toque filtrado por la interfaz (`suppressed-ui-or-contact`). SHA-256 del JSONL al exportar a las 12:16:23Z: `ba521c14bd9e8a3aeb0a841c1e949aad1183f9ecb9d90d5604be0fd4c3c0a36a`.

### Preguntas y respuestas literales

Enviadas después de «Listo termine S3-A» (12:00:47Z) y antes de mostrar resultados, en tres diálogos seguidos; respuestas registradas a las 12:02:43Z, 12:11:58Z y 12:12:21Z. Cada pregunta ofrecía «No recuerdo» y «Nada que señalar»; la respuesta libre iba en «Other». Antes de las preguntas se informó al PO de que faltaba la marca de DEV, sin datos del bloque.

| Pregunta | Respuesta |
|---|---|
| Por número de recepción, ¿qué excepciones recuerdas: Apoyo sin querer, No fijó / falló u otra dificultad? | «Todas ok, hice mas de 6» |
| Por número de recepción, ¿qué sonido oíste, si oíste alguno? | «No escuché ningún sonido» |
| Por número de recepción, ¿qué vibración sentiste, si sentiste alguna? | «Nada que señalar» (opción) |
| Por número de recepción, ¿qué nota, destello o movimiento de la raqueta viste, si viste alguno? | «Veía como se movía la raqueta al intentar golpear» |
| ¿Hubo alguna recepción sin contacto en que percibiste sonido de impacto, vibración, destello o pose de contacto? Si recuerdas el número, indícalo. | «Nada que señalar» (opción) |
| Si hubo un intento al aire o tardío, ¿qué sonido y nota percibiste, si percibiste alguno? | «No hubo sonido» |
| Por número de recepción, ¿cuándo viste empezar el movimiento de la raqueta respecto de la suelta y qué viste después? | «Nada que señalar» (opción) |
| Fuera de las preguntas del bloque: ¿abriste y cerraste DEV antes de empezar S3-A? | «No, empecé directo» (opción) |
| ¿Un perfecto se siente claramente mejor? | «No recuerdo» (opción) |
| ¿Entendiste el resultado del golpe sin el panel? | «Nada que señalar» (opción) |

**Aclaración de la pregunta 3**, pedida antes de mostrar resultados porque «Nada que señalar» era ambiguo. Opciones: «No sentí vibración», «Sí vibró, sin más», «No recuerdo». Respuesta, registrada a las 12:15:58Z: **«Sí vibró, sin más»**. El gesto extra de LaunchId 108 (12:15:41Z) ocurrió mientras el PO respondía esta aclaración.

Lectura descriptiva, sin reinterpretar: el PO no oyó sonido en ninguna recepción, sintió vibración y vio moverse la raqueta. El contraste Perfect/Valid quedó sin juicio («No recuerdo») y las preguntas 7 y de comprensión, sin contenido. Ninguna respuesta señala feedback de contacto sin contacto.

## 4. Parada y diagnóstico

Ausencia total de sonido con timbre activo y 8 peticiones de audio (5 impactos y 3 swish) → parada antes de S3-B, conforme a §7 de S2 y §8 de la predeclaración.

**Diagnóstico estático de Claude, pendiente de confirmar con pruebas:** `Main.unity`, única escena de la build, tiene una sola cámara («Main Camera») **sin `AudioListener`**. No hay ningún `AudioListener` en `Assets/` ni en `ProjectSettings/`, ni código que lo añada. Sin listener, Unity no reproduce audio. Origen probable: `Editor/ProjectSetup.cs:89-97` crea la cámara en una escena vacía con `AddComponent<Camera>()`. Las pruebas no lo detectaron porque `ShotFeedback.Play` omite `PlayOneShot` en batch mode y las pruebas cuentan peticiones. Probablemente afectó también al sonido provisional de S2 (S2-OBS-05, que quedó NotEvaluated).

`AudioManager` tiene `m_DisableAudio: 0` y volumen 1; `muteOtherAudioSources: 0`. La vibración, que no depende del listener, se sintió.

## 5. Restauración

- 12:16:23Z, export `restore-check`: proceso `115246125` aún activo con P0, `6ecfdfa78cbb` y `pending=0`; **los tres archivos idénticos al respaldo** por `cmp` y SHA-256. No hubo configuración temporal: S3-E no se llegó a preparar y los ajustes del teléfono no se cambiaron.
- **Verificación en proceso nuevo pendiente.** Dos relanzamientos rechazados con el iPhone bloqueado (`FBSOpenApplicationErrorDomain` 7, `Locked`), hacia las 12:16Z y en un segundo intento. Se completa cuando el PO desbloquee el iPhone.
- Queda instalada la build `0.1.0+fab60ef dev` en lugar de la PROD de S2.

## 6. Siguiente paso, según la decisión del PO

Codex prepara una PR independiente que corrige solo el audio, con una prueba que falle antes de la corrección. Claude la revisa de forma independiente y el PO aprueba la fusión. Después: build nueva, prueba breve de sonido de impacto con timbre activo y, si el audio funciona, **repetición completa de S3-A–E** con el protocolo predeclarado y la marca de DEV. Las recepciones de hoy no cuentan para la nueva cuota.
