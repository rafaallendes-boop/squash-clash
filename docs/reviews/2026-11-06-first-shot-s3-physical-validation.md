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
- **Verificación en proceso nuevo.** Dos relanzamientos hacia las 12:16Z y 12:23Z fueron rechazados con el iPhone bloqueado (`FBSOpenApplicationErrorDomain` 7, `Locked`). Tras el aviso del PO «Desbloqueado y abierto en duplicar», el relanzamiento de las 12:27Z funcionó. Proceso nuevo `metrics-20261106T122711061-b4af5cb7a8fe40e797dfa5f566920a3c.jsonl`, arranque **2026-11-06T12:27:11.0788920Z**: `0.1.0+fab60ef dev`, `P0-calib-1`, **`6ecfdfa78cbb`**, `pending=0`. Export a las 12:27:24Z: **los tres archivos idénticos por `cmp` y SHA-256 al respaldo y a la referencia inicial** después del arranque. **Restauración verificada.**
- Queda instalada la build `0.1.0+fab60ef dev` en lugar de la PROD de S2.

## 6. Siguiente paso, según la decisión del PO

Codex prepara una PR independiente que corrige solo el audio, con una prueba que falle antes de la corrección. Claude la revisa de forma independiente y el PO aprueba la fusión. Después: build nueva, prueba breve de sonido de impacto con timbre activo y, si el audio funciona, **repetición completa de S3-A–E** con el protocolo predeclarado y la marca de DEV. Las recepciones de hoy no cuentan para la nueva cuota.

## Continuación con audio corregido · 6 de noviembre de 2026 (Chile)

**Fuente.** `firstshot/s3-validation` integra `main` en `27058e8`. Producto idéntico a ese main. Predeclaración de la prueba breve y alcance en `097eab9`, anterior a la build y al acceso al iPhone. La [adenda](../validation/2026-11-06-first-shot-s3-physical-predeclaration.md#adenda-del-6-de-noviembre-de-2026--prueba-breve-de-sonido-y-repetición-con-audio-corregido) mantiene separado el S3-A parcial anterior.

**Build.** `check-release` DEV y PROD frescos desde `097eab9` con `gitDirty=false`. Inspección de ambos `Data/level0`: una cámara principal activa y un `AudioListener` habilitado en el mismo GameObject. Los SHA-256 coinciden con los registrados por [PR #35](2026-11-06-first-shot-s3-audio-listener.md). `scripts/build-ios.sh dev` produjo `0.1.0+097eab9 dev` Release ARM64, IL2CPP Release, no Development y con `SQUASH_DEV`. El `Data/level0` firmado es idéntico al del export DEV comprobado. Firma estricta y perfil incorporado cotejados antes de instalar, con vencimiento `2026-11-12T13:55:07Z` (10:55 Chile). `ProjectSettings/` coincide byte a byte con el original. Se conserva la regresión revisada de PR #35 (546/546 EditMode y 354/354 PlayMode).

**Respaldo e instalación.** `Documents` exportado de la DEV anterior `0.1.0+fab60ef dev`, último arranque P0 a las 12:27:11Z. Los tres archivos protegidos coincidieron por bytes y SHA-256 con la referencia inicial. Se instaló sobre la app existente. Antes de abrir, se volvieron a cotejar los tres archivos y se comprobó que aún no había un proceso nuevo en los logs. Primer arranque corregido a las **14:03:12.0095980Z**, `0.1.0+097eab9 dev`, `P0-calib-1`, `6ecfdfa78cbb`, cero pendientes. Los tres archivos volvieron a coincidir con respaldo y referencia inicial.

**Punto de control previo a la prueba breve.** El PO indicó «Listo a mano y conectado en duplicar». Se pidió cerrar Duplicación y confirmar el uso en mano, timbre y volumen multimedia audible antes de iniciar. La exportación de espera verificó, en el mismo proceso, `LaunchId 49` a las 14:06:28.5909160Z con hash `6ecfdfa78cbb`, cero pendientes, cero gestos y los tres archivos protegidos coincidentes. Esta exportación es preparación. La prueba breve y las nuevas cuotas S3-A–E están pendientes de ejecución.

### Prueba breve de sonido

El PO declaró «Duplicación cerrada, iPhone en mano, timbre activo y volumen multimedia audible» (registro 14:44:41Z). Proceso nuevo de la build corregida desde **14:38:30.3942540Z**, P0-calib-1 `6ecfdfa78cbb`, cero pendientes. La exportación antes de la señal verificó los tres archivos contra ambos respaldos. Los gestos anteriores a la frontera quedaron como preparación.

**Frontera.** Cierre DEV aceptado y completado, línea 422 del proceso, **14:45:18.0549020Z**, `timeReal=409.815659458`, `focused=true`, `paused=false`, `devOpen=false`. Señal registrada en Mac a las 14:45:00Z y aviso de fin a las 14:46:29Z. Recepciones de la ventana predeclarada: LaunchId 98–103, con hash `6ecfdfa78cbb` y cero pendientes.

| Recepción | LaunchId | Gesto | Resultado final / motivo | Aplicado | Presentación |
| --- | ---: | --- | --- | --- | --- |
| 1 | 98 | Swing | Valid / applied | Sí | Valid, v2 |
| 2 | 99 | Swing | Valid / applied | Sí | Valid, v2 |
| 3 | 100 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 4 | 101 | Swing | Valid / applied | Sí | Valid, v2 |
| 5 | 102 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 6 | 103 | Swing | Failed / outside-v | No | Failed / Air, v2 |

Cada intento de la ventana tiene una presentación concordante y no hay discrepancias. **Desviación:** el PO dijo «Listo, hice 9 repeticiones en total». El log registra diez Swing tras cerrar DEV hasta la exportación de fin. LaunchId 104–107 son extras: tres Failed / Air y un Valid aplicado. El primer contacto aplicado ya consta en LaunchId 98. Se conservaron el exceso y la diferencia entre recuentos. La prueba breve completa, incluidos los extras, queda fuera de S3-A–E. Export de fin a las 14:46:37Z, SHA-256 del proceso `891030be7e7446fe7d8cc0e0b9d73707e06f64a1533912479a5f0df478818264`. Los tres archivos protegidos volvieron a coincidir.

**Respuestas literales, antes de mostrar resultados.** A «¿Oíste un sonido de impacto al golpear?», el PO respondió «Sí» (registro 14:47:25Z). Al precisar cuándo, respondió «En cada swing que intentaba hacer, sonaba el impacto» (14:47:54Z). La aclaración neutral sobre si sonaban iguales obtuvo «Distinguí sonidos diferentes» (14:48:05Z), seguida de «Un sonido al golpear exitosamente y otro cuando se me iba al aire» (registro 14:49:04Z). Queda acreditado audio audible y distinción global de los dos sonidos en la build corregida, con contactos aplicados dentro de la ventana. La percepción por recepción no fue identificada. La aclaración separa el sonido del intento al aire del que el PO atribuye al golpe exitoso.

**Decisión del PO.** «Sí, autorizo iniciar S3-A». Estado del teléfono: «Timbre activo, volumen multimedia audible y háptica del sistema activa» tras la respuesta inicial «Estamos ok». El primer relanzamiento para S3-A fue rechazado porque el iPhone se bloqueó. Se pidió desbloquearlo para repetir el precheck. No se emitió señal de inicio de S3-A en ese intento.

### S3-A · Contacto

El PO confirmó que el iPhone estaba desbloqueado. El segundo relanzamiento abrió un proceso nuevo a las **14:53:01.6468410Z**, `0.1.0+097eab9 dev`, P0-calib-1, `6ecfdfa78cbb` y cero pendientes. El precheck verificó el plan y los tres archivos, con cero gestos. El cierre DEV aceptado y completado quedó en la línea 47 a las **14:53:40.2214280Z**, `timeReal=40.711298667800456`, con foco, sin pausa y DEV cerrado. Señal Mac 14:53:11Z. El PO avisó «Terminé S3-A» a las 14:54:47Z y se exportó antes de relanzar.

| Recepción | LaunchId | Gesto | Resultado final / motivo | Aplicado | Presentación |
| --- | ---: | --- | --- | --- | --- |
| 1 | 12 | Swing | Valid / applied | Sí | Valid, v2 |
| 2 | 13 | Swing | Valid / applied | Sí | Valid, v2 |
| 3 | 14 | Swing | Valid / applied | Sí | Valid, v2 |
| 4 | 15 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 5 | 16 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 6 | 17 | Swing | Failed / outside-v | No | Failed / Air, v2 |

Los planes y gestos de cuota tienen hash `6ecfdfa78cbb` y cero pendientes. Cada intento tuvo una presentación concordante, con cero discrepancias. **Fuera de cuota:** cinco Swing adicionales en LaunchId 18–22, cuatro Failed / Air y un Valid aplicado. El export de fin confirmó el prefijo del precheck y los tres archivos iguales al respaldo y a la referencia inicial. SHA-256 del proceso exportado `8d397eb491b574e50483b7550479f44ffec14c001802134cc1268bd520b08e92`. El contraste Perfect/Valid queda NotEvaluated en este bloque por falta de Perfect en la cuota.

**Respuestas literales de S3-A.** Q4 (2026-11-06T14:58:58Z): «Veía como se movia la raqueta azul», sin desglose por recepción.

El PO respondió después en ambos diálogos de los campos 1–4 (registro 14:59:20Z). Se conservan ambas series, sin desglose por recepción.

| Campo | Diálogo original | Diálogo de campos pendientes |
| --- | --- | --- |
| 1 · Excepciones | «Nada que señalar» | «todo ok» |
| 2 · Sonido | «El sonido del golpe» | «el sonido del golpe» |
| 3 · Vibración | «VIbrar al intentar realizar el golpe» | «la vibración al intentar dar un golpe» |
| 4 · Vista | «veía como se movia la raqueta azul» | — |

Campos 5–7 (2026-11-06T15:00:04Z), respectivamente: «Nada que señalar», «Se veía como un sonido de que fallaste» y «Se ve el movimiento de la raqueta al intentar golpear».

Preguntas extra (2026-11-06T15:00:30Z). «¿Un perfecto se siente claramente mejor?» → «Nada que señalar». «¿Entendiste el resultado del golpe sin el panel?» → «si». Ante la aclaración neutral sobre vibración al irse al aire, respondió «Solo recuerdo vibración al hacer contacto».

### S3-B · Intento tras bloqueo, conservado para revisión

El PO declaró «Timbre activo, volumen multimedia audible y háptica del sistema activa». Precheck en proceso nuevo a las **15:00:47.3728180Z**, P0-calib-1, `6ecfdfa78cbb`, cero pendientes, sin gestos y con los tres archivos cotejados. Señal Mac 15:00:53Z. El export de monitoreo terminaba a las 15:01:11.4929510Z sin marca DEV ni gestos. A las 15:02:19Z el iPhone reportó `passcodeRequired=true`. Se retiró la señal y se pidió desbloquear y esperar un precheck nuevo. El PO informó a las 15:04:14Z «Terminé S3-B fueron 8 golpes ya que el golpe 7 y 8 hice lo que pedía el golpe 6» y «Está desbloqueado, puedo seguir ahora».

El export mostró el mismo proceso previo al bloqueo. Tras reanudarse quedó un cierre DEV inequívoco en línea 38 a las **15:02:48.4243410Z**, `timeReal=123.1789326707347`, foco activo, sin pausa, DEV cerrado. La frontera permite describir lo ocurrido. El bloque requiere un nuevo precheck tras el bloqueo conforme a §3.2 de S2, por lo que este intento se conserva fuera de una cuota válida. El LaunchId 8 ya estaba en curso al cerrar DEV y registró una cancelación fuera de la ventana. Primer LaunchId posterior al cierre: 9.

| Posición tras la frontera | LaunchId | Gesto | Resultado final / motivo | Aplicado | Presentación |
| --- | ---: | --- | --- | --- | --- |
| 1 | 9 | CanceledReturnToDeadZone | NoOrder / fin sin orden | No | — |
| 2 | 10 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 3 | 11 | AbandonedNoPull | NoOrder / fin sin orden | No | — |
| 4 | 12 | Swing | Failed / before-k-in | No | Failed / Air, v2 |
| 5 | 13 | AbandonedNoPull | NoOrder / fin sin orden | No | — |
| 6 | 14 | Swing | Failed / after-k-out | No | Failed / Late, v2 |

Planes y gestos con hash `6ecfdfa78cbb`, cero pendientes. La ventana no contiene contactos aplicados, las tres presentaciones tienen Failed final y no hay discrepancias. Después de esas seis recepciones se registraron cuatro Swing fuera de ventana (LaunchId 15 y 18–20) y dos NoOrder (16–17). El registro tiene un intervalo sin líneas entre 15:03:21.6180570Z y 15:03:57.5625040Z. Se conserva el recuento declarado por el PO separado del observado. Export de fin con SHA-256 `1e2a2ff697272ff0c359d4de21ebed3f580e984218e5788a376aeb62ec6c85fe` y tres archivos cotejados.

**Respuestas literales del intento S3-B.** Campos 1–4 (2026-11-06T15:05:34Z), respectivamente: «Todo ok», «Sonido al intentar golpear», «Nada que señalar» y «Vi como se mueve la raqueta al intentar golpear».

Campos 5–7 (2026-11-06T15:06:49Z): «Nada que señalar» en los tres.

El PO aclaró «Como el sonido del intento al aire» y confirmó «iPhone desbloqueado y ajustes iniciales repuestos». Se exportó antes de relanzar. Restauración verificada en proceso nuevo **15:07:20.5700240Z**, build `0.1.0+097eab9 dev`, P0-calib-1 `6ecfdfa78cbb`, cero pendientes y tres archivos cotejados contra ambos respaldos antes y después. El PO autorizó de forma expresa «Sí, autorizo repetir S3-B ahora». Se hizo un nuevo export de precheck en el proceso recién restaurado, plan LaunchId 9 a las 15:07:49.8770740Z, P0, cero pendientes, cero gestos y tres archivos cotejados. Señal Mac 15:07:53Z. El intento anterior queda separado.
