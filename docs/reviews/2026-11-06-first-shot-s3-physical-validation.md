# First Shot S3 — validación física S3-A–E

Título anterior, conservado como histórico: «First Shot S3 — validación física, sesión detenida tras S3-A». Las secciones 1–6 describen la primera sesión del 6 de noviembre, detenida por falta de audio. Las siguientes, la continuación con el audio corregido.

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

### S3-B · Repetición autorizada

Cierre DEV aceptado y completado en línea 67, **15:08:19.9638910Z**, `timeReal=61.52772345823357`, foco activo, sin pausa y DEV cerrado. Los planes avanzaron entre el precheck y la frontera. El PO avisó «Terminé S3-B» y se exportó antes de relanzar.

| Recepción | LaunchId | Gesto | Resultado final / motivo | Aplicado | Presentación |
| --- | ---: | --- | --- | --- | --- |
| 1 | 17 | CanceledReturnToDeadZone | NoOrder / fin sin orden | No | — |
| 2 | 18 | CanceledReturnToDeadZone | NoOrder / fin sin orden | No | — |
| 3 | 19 | Swing | Failed / before-k-in | No | Failed / Air, v2 |
| 4 | 20 | AbandonedNoPull | NoOrder / fin sin orden | No | — |
| 5 | 21 | Swing | Failed / before-k-in | No | Failed / Air, v2 |
| 6 | 22 | Swing | Failed / after-k-out | No | Failed / Late, v2 |

Tres presentaciones, todas con Failed final y sin contactos aplicados. Cero discrepancias. Los planes y gestos cotejaron `6ecfdfa78cbb`, cero pendientes. LaunchId 23 aparece como plan fuera de cuota, sin gesto en el export de fin. Los tres archivos coinciden con respaldo y referencia inicial. SHA-256 `3b977bcf84b1504dfc6bb5824a89162d0466dce69d9c1938c0a0c47dae2a4856`.

**Respuestas literales.** Campos 1–4 (2026-11-06T15:09:52Z): «todo ok», «en las que pego al aire suena como que le pegaste al aire», «no se sintió» y «se mueve la raqueta al intentar golpear».

Campos 5–7 (2026-11-06T15:10:44Z): «Nada que señalar», «Nada que señalar» y «cuando se intenta golpear la pelota, se mueve la raqueta».

### S3-C · Preparación detenida antes de la frontera

El PO confirmó «Modo silencio activado, app en primer plano». Se exportó antes de relanzar. Proceso nuevo a las **15:11:13.1167620Z**, `0.1.0+097eab9 dev`, P0-calib-1 `6ecfdfa78cbb`, cero pendientes, cero gestos y los tres archivos cotejados. Se envió la consigna de S3-C. En el export de monitoreo el registro termina en una línea de frames a las **15:11:37.2186140Z**, sin cierre DEV ni gestos. Una consulta posterior confirmó `passcodeRequired=true`. Se retiró la señal, se conservó la exportación y se pidió al PO informar si llegó a jugar, mantener el iPhone desbloqueado y esperar un nuevo precheck. El monitoreo disponible no acredita el inicio de S3-C.

C, D, E y la observación de pausa quedan pendientes en este punto de control. El último arranque cotejado es P0 en la preparación de C. La restauración de cierre de la sesión y la reposición del modo silencio quedan pendientes de coordinación con el PO. No se abrió todavía la PR de validación.

### S3-C · Intento tras bloqueo, conservado para revisión

El PO informó «Termine S3 - C . Los primeros 3 golpes fueron al aire y los 4 siguientes logré golppear 3» y «Sí jugué después de la señal anterior». Se exportó el mismo proceso del precheck previo al bloqueo, antes de relanzar. El cierre DEV posterior quedó en línea 37 a las **15:21:53.9935070Z**, `timeReal=643.046550458567`, foco activo, sin pausa y DEV cerrado. El intento ocurrió sin precheck nuevo tras el bloqueo y se conserva separado de una cuota válida.

| Posición tras la frontera | LaunchId | Gesto | Resultado final / motivo | Aplicado | Presentación |
| --- | ---: | --- | --- | --- | --- |
| 1 | 9 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 2 | 10 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 3 | 11 | Swing | Failed / outside-v | No | Failed / Air, v2 |

LaunchId 12–15 son extras, con tres Valid aplicados (12, 13 y 15) y un Failed / Air (14). Los siete intentos cotejan `6ecfdfa78cbb`, cero pendientes, una presentación concordante por intento y cero discrepancias. Los tres archivos cotejan ambos respaldos. SHA-256 del proceso en el export `544f7984ecb17f8825abff5f3defda6ce52b97f11de044cfb01a3c5bf77aa9eb`.

Se solicitó el primer diálogo de respuestas del intento antes de mostrar el desglose. Al guardar este punto de control, las respuestas, la restauración de P0 tras el intento y la decisión de repetir S3-C seguían pendientes.

**Respuestas literales del intento S3-C.** Campos 1–4 (2026-11-06T15:33:34Z): «todas ok», «no hubo ya que estaba en silencio», «vibraba en los golpes» y «se movia al intentar golpear».

Campos 5–7 (2026-11-06T15:35:08Z): «Nada que señalar» en los tres.

El PO confirmó «Timbre activo, iPhone desbloqueado y app en primer plano» y autorizó «Sí, autorizo repetir S3-C ahora». Antes del relanzamiento se exportó y se cotejaron los tres archivos. P0 restaurado en proceso nuevo **15:35:39.8880820Z**, build `0.1.0+097eab9 dev`, hash `6ecfdfa78cbb`, cero pendientes y tres archivos cotejados de nuevo contra el respaldo y la referencia inicial. El intento previo y sus respuestas quedan separados.

**Repetición autorizada, detenida en preparación.** El PO volvió a confirmar «Modo silencio activado, app en primer plano». Proceso nuevo a las **15:36:36.2332220Z**, P0-calib-1 `6ecfdfa78cbb`, cero pendientes y tres archivos cotejados. Después de la señal el teléfono se bloqueó otra vez (`passcodeRequired=true`). Los dos exports de monitoreo tienen el mismo proceso y bytes, última línea a las **15:37:00.3374140Z**, sin cierre DEV ni gestos. Se retiró la señal. S3-C sigue pendiente de un precheck nuevo antes de jugar. El último arranque comprobado conserva P0 y los tres archivos. El estado final del modo silencio queda pendiente de confirmación del PO.

### S3-C · Repetición posterior al segundo bloqueo

El PO informó «Terminé S3-C» y «Sí, ya jugué la repetición». El export posterior conserva como prefijo exacto los registros del monitoreo anterior. En el mismo proceso de las 15:36:36.2332220Z se registró un cierre DEV posterior al bloqueo, línea 43 a las **16:06:58.1913190Z**, `timeReal=1824.1082705890522`, foco activo, sin pausa y DEV cerrado. El cierre permite reconstruir las tres recepciones. El intento queda fuera de cuota porque ocurrió tras el bloqueo y sin un precheck nuevo.

| Posición tras la frontera | LaunchId | Gesto | Resultado final / motivo | Aplicado | Presentación |
| --- | ---: | --- | --- | --- | --- |
| 1 | 10 | Swing | Valid / applied | Sí | Valid, v2 |
| 2 | 11 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 3 | 12 | Swing | Failed / outside-v | No | Failed / Air, v2 |

Las tres recepciones cotejan `6ecfdfa78cbb`, cero pendientes y una presentación concordante por intento, con cero discrepancias. LaunchId 13 aparece fuera de ventana. Los tres archivos cotejan ambos respaldos. SHA-256 del proceso en el export `6dee04a6c77b603f70f9e219733982a79c5b846f22001d97fcf5816dac20a3ae`.

**Respuestas literales.** Campos 1–4 (2026-11-06T16:08:22Z): «todo ok», «Nada que señalar», «vibración al golpear» y «Nada que señalar».

Campos 5–7 (2026-11-06T16:08:41Z): «Nada que señalar» en los tres.

### S3-C · Repetición válida en modo silencio

El PO repuso el timbre y eligió «Repetir S3-C ahora con más tiempo antes del bloqueo». Se verificó restauración en proceso nuevo a las **16:09:00.7916160Z**, P0-calib-1, `6ecfdfa78cbb`, cero pendientes y tres archivos iguales antes y después. La primera exportación posterior falló con CoreDeviceError 7000 y el reintento completo permitió el cotejo. Ante la propuesta de alargar el bloqueo automático, el PO dijo «dejaré el iphone abierto». Confirmó «Modo silencio activado y pantalla encendida con Squash Clash». Se preparó otro proceso a las **16:09:52.7672200Z** con P0, cero pendientes, cero gestos y cotejo de archivos antes de la señal.

Cierre DEV en línea 31, **16:10:14.6949020Z**, `timeReal=24.066158791099777`, foco activo, sin pausa, DEV cerrado. El PO avisó «listo, los 3 salieron al aire eso si».

| Recepción | LaunchId | Gesto | Resultado final / motivo | Aplicado | Presentación |
| --- | ---: | --- | --- | --- | --- |
| 1 | 8 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 2 | 9 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 3 | 10 | Swing | Failed / outside-v | No | Failed / Air, v2 |

Planes y gestos con `6ecfdfa78cbb`, cero pendientes, una presentación concordante por intento y cero discrepancias. No hay otros gestos en el export. Los tres archivos cotejaron los dos respaldos. SHA-256 del proceso `dd7966a08a48e8863ebae463cf2b6bfcc3e5e8633743df6027d9fc62efaa8264`. Campos 1–7: «Nada que señalar» en cada uno. En esta cuota se observó silencio sin contacto según lo informado. Silencio durante un impacto aplicado queda **NotEvaluated** en la cuota válida. Las observaciones de los intentos tras bloqueo se conservan aparte.

### S3-D · Preparación detenida antes de la frontera

El PO declaró «Timbre activo, volumen audible y háptica del sistema desactivada». Proceso nuevo a las **16:11:31.3169610Z**, build `0.1.0+097eab9 dev`, P0-calib-1 `6ecfdfa78cbb`, cero pendientes, cero gestos al precheck y tres archivos cotejados. Se envió la consigna. Los exports de monitoreo muestran un gesto Rejected en preparación, sin cierre DEV. La última línea del segundo monitoreo es de las **16:13:27.7849300Z**. La consulta posterior confirmó `passcodeRequired=true`. Se retiró la señal y se pidió al PO informar si llegó a jugar y esperar un nuevo precheck con el teléfono desbloqueado.

**Pausa por decisión del PO.** «Dejemos en pausa mientras y después retomamos». El export de pausa conservó el mismo proceso de D, sin marca DEV y con un gesto Rejected de preparación. Los tres archivos coinciden byte a byte y por SHA-256 con respaldo y referencia inicial. A la solicitud de reponer los ajustes del teléfono y comprobar P0 en otro proceso, el PO respondió «Dejemos la restauración para cuando retomemos». Queda pendiente reponer la háptica del sistema y verificar P0 en un proceso nuevo. La última configuración del teléfono declarada fue timbre activo, volumen audible y háptica del sistema desactivada. A, B y C tienen cuotas válidas. D, E, observación de pausa y restauración final quedan pendientes. La PR de validación sigue sin abrir.

## Continuación operada por Claude · 9 de noviembre de 2026 (Chile)

El PO asignó la operación a Claude («Claude, ahora»). Se retoma tras la pausa con los mismos helpers, respaldos y carpeta de evidencia. Antes de cualquier bloque se exporta el estado del teléfono, se cotejan los tres archivos y se prepara un proceso nuevo en P0 con precheck completo. El intento de S3-D del 6 de noviembre, sin frontera, queda como preparación.

**Estado al retomar.** Export `resume-before-D2` con el iPhone desbloqueado: tres archivos iguales al respaldo y a la referencia inicial. Después de la pausa del 6 de noviembre la app se abrió tres veces fuera de la sesión: 2026-11-06T20:13:50Z (7 gestos), 2026-11-07T00:36:11Z (2 gestos) y 2026-11-10T00:09:35Z (sin gestos). Las tres estaban en P0-calib-1, `6ecfdfa78cbb`, cero pendientes y sin controles DEV. Se conservan como preparación, fuera de cuota.

### S3-D · Háptica del sistema desactivada

Proceso nuevo a las **2026-11-10T00:10:18.5123010Z**, `0.1.0+097eab9 dev`, P0-calib-1, `6ecfdfa78cbb`, cero pendientes y cero gestos. Tres archivos cotejados. Precheck aceptado. Declaración del PO, por diálogo y después del precheck: «Timbre activo, volumen audible y háptica del sistema desactivada». Señal en Mac a las 00:12:36Z.

Cierre DEV en la línea 90, **00:14:31.2495560Z**, `timeReal=254.86540617228573`, con foco, sin pausa y DEV cerrado. El PO avisó «Terminé S3-D».

| Recepción | LaunchId | Gesto | Resultado final / motivo | Aplicado | Presentación |
| --- | ---: | --- | --- | --- | --- |
| 1 | 16 | Swing | Valid / applied | Sí | Valid, v2 |
| 2 | 17 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 3 | 18 | Swing | Failed / outside-v | No | Failed / Air, v2 |

Planes y gestos con `6ecfdfa78cbb` y cero pendientes. Una presentación concordante por intento, sin discrepancias. LaunchId 15 estaba en curso al cerrar DEV y registró un Rejected después del cierre, fuera de la ventana. LaunchId 19–21 son planes sin gesto. Tres archivos cotejados. SHA-256 del proceso `50eb5ae717adc891da035a236372cd30126e177e7c7cf5a18d30c63cf2533db4`.

**Respuestas literales**, antes de mostrar resultados. Campos 1–4: «Nada que señalar», «Sonido al golpear la pelota», «Vibración al golpear la pelota» y «Nada que señalar». Campos 5–7: «Nada que señalar» en los tres. A la aclaración neutral sobre qué ajuste se desactivó, respondió «Sonidos y vibraciones → Háptica del sistema».

**Observación.** El PO percibió vibración en el golpe con «Háptica del sistema» desactivada. La recepción 1 tiene contacto aplicado. El registro no permite saber si iOS reproduce la háptica de la app con ese ajuste apagado. Queda abierta, sin atribuir causa.

### S3-E · Interruptor de la app

Proceso nuevo a las **2026-11-10T00:18:14.9825770Z** en P0, `6ecfdfa78cbb`, cero pendientes, cero gestos y tres archivos cotejados. Declaración del PO: «Timbre activo, volumen audible y háptica del sistema activa».

**Primer precheck rechazado.** El PO abrió y cerró DEV (00:19:40Z–00:19:49Z), pero no se registró el despliegue del grupo de parámetros y los planes siguieron en `6ecfdfa78cbb`. No se dio la señal. Se indicó la ruta: pestaña de parámetros, grupo «First Shot / feedback». El PO respondió «Lo encontré, lo apagué y cerré DEV».

**Segundo precheck, aceptado, en el mismo proceso.** Se registraron el despliegue de «First Shot / feedback» a las 00:20:42Z, el cambio de `feedback.haptics` a las 00:20:44.026Z y el cierre de DEV a las 00:20:44.882Z. Desde el cambio, el hash es **`4f33dff747fb`**, igual al literal predeclarado. El último plan antes de la señal fue LaunchId 48, con cero pendientes y sin pausa. Un Rejected a las 00:20:22Z, anterior al cambio, es preparación. Tres archivos cotejados. El script `precheck.py` exige que el proceso sea nuevo respecto al export que recibe como anterior. Aquí se le pasó `after-D2`, porque el cambio de S3-E ocurre en el mismo proceso del precheck en P0. Señal en Mac a las 00:21:53Z.

Cierre DEV en la línea 906, **00:22:10.7191790Z**, `timeReal=237.87285262518367`, `activeLaunchHash=4f33dff747fb`, con foco, sin pausa y DEV cerrado. El PO avisó «Terminé S3-E».

| Recepción | LaunchId | Gesto | Resultado final / motivo | Aplicado | Presentación |
| --- | ---: | --- | --- | --- | --- |
| 1 | 51 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 2 | 52 | Swing | Failed / outside-v | No | Failed / Air, v2 |
| 3 | 53 | Swing | Failed / outside-v | No | Failed / Air, v2 |

Planes y gestos con `4f33dff747fb` y cero pendientes. Una presentación concordante por intento, sin discrepancias. LaunchId 54 es un plan sin gesto. Tres archivos cotejados. SHA-256 del proceso `5b8a82634f221568b96d7f356c35216093f4f7654f2dfd88335d0bde6905ea00`.

**Respuestas literales.** Campos 1–7: «Nada que señalar» en cada uno. A la aclaración neutral «¿oíste algún sonido?», respondió «Sí, oí sonido».

**NotEvaluated.** En la cuota no hubo contacto aplicado. Por lo tanto, no se evaluó si `feedback.haptics=0` elimina la vibración de un golpe. No se repitieron recepciones para conseguir un contacto. El apagado no persistió: el siguiente proceso nuevo arrancó en `6ecfdfa78cbb`.

### Observación de pausa, fuera de cuota

El primer relanzamiento falló porque el iPhone estaba bloqueado. Tras el desbloqueo se abrió un proceso nuevo a las **2026-11-10T00:28:54.1207750Z** en P0, `6ecfdfa78cbb`, cero pendientes, cero gestos y tres archivos cotejados.

Secuencia registrada:

| Hora (Z) | Evento |
| --- | --- |
| 00:30:24 / 00:30:29 | DEV abierto y cerrado |
| 00:30:32 | Swing en LaunchId 18, Failed / outside-v, presentación Failed / Air |
| 00:30:39.350 | Pausa, siete segundos después del swing, durante LaunchId 20 sin gesto |
| 00:30:45.647 | Reanudación |
| Después | Rejected (L20), Swing Failed / before-k-in con Failed / Air (L21) y Rejected (L22) |

El proceso no tiene ninguna presentación Perfect ni contactos aplicados. SHA-256 `78620d40b83629913935847a113edebe17529c751cbb4a13c4da7c150cd24c93`.

**Respuesta literal** a la pregunta de §5: «Se vio la raqueta, el sonido y el destello». A la aclaración neutral sobre el momento del destello: «Al golpear».

**Parada de §8.** El PO percibió un destello al golpear sin contacto aplicado. El destello solo corresponde a un Perfect, y el registro no tiene ninguno. Se cumple la condición de parada de §8 por percepción del destello sin aplicación. No se hicieron más gestos y se pasó a la restauración, que era el paso siguiente. El registro no distingue si fue otro elemento visual o un destello real; queda abierta sin atribuir causa. La pausa no llegó justo después de un golpe con contacto, así que la congelación de raqueta y destello en pausa queda **NotEvaluated en el iPhone**. Conserva la cobertura de Editor ya registrada.

### Restauración final

El PO declaró «Listo, repuestos». Antes del relanzamiento, los tres archivos del export `after-pause` coincidían con el respaldo y la referencia inicial. Proceso nuevo a las **2026-11-10T00:33:41.2169830Z**, `0.1.0+097eab9 dev`, P0-calib-1, **`6ecfdfa78cbb`, cero pendientes** y cero gestos. Después del arranque, P0-calib-1, Po-1 y last-active seguían iguales byte a byte y por SHA-256 al respaldo y a la referencia inicial. SHA-256 del proceso `2246aada484d4c56da9510406c1fd4a3587ad81d25c99c849716f581c52f828f`. Queda instalada la build DEV `0.1.0+097eab9 dev` y no se reinstaló nada.

## Resumen de las cuotas válidas S3-A–E

| Bloque | Operador | Recepciones | Contactos aplicados | Perfect | Discrepancias | Hash |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| S3-A · Contacto | Codex | 6 | 3 | 0 | 0 | `6ecfdfa78cbb` |
| S3-B · Sin contacto e intención (repetición) | Codex | 6 | 0 | 0 | 0 | `6ecfdfa78cbb` |
| S3-C · Silencio (repetición válida) | Codex | 3 | 0 | 0 | 0 | `6ecfdfa78cbb` |
| S3-D · Háptica del sistema desactivada | Claude | 3 | 1 | 0 | 0 | `6ecfdfa78cbb` |
| S3-E · Interruptor de la app | Claude | 3 | 0 | 0 | 0 | `4f33dff747fb` |
| **Total** | | **21** | **4** | **0** | **0** | |

**Acreditado por el registro.** En las 21 recepciones hubo una presentación como máximo por intento, siempre concordante con el resultado final. No se presentó nada para cancelaciones, rechazos ni NoOrder, y no hubo discrepancias. Los hashes coinciden con los literales predeclarados y todo quedó con cero pendientes.

**Acreditado por el PO.** Audio audible con impacto aplicado y dos sonidos distinguidos (prueba breve). Sonido y vibración en el golpe (S3-A y S3-D). Sonido de intento al aire (S3-B). Movimiento de la raqueta al intentar golpear.

**NotEvaluated:**
- contraste Perfect/Valid y destello de Perfect, porque las cuotas no tuvieron ningún Perfect;
- silencio durante un impacto aplicado (S3-C);
- `feedback.haptics=0` con contacto aplicado (S3-E);
- congelación de feedback en pausa en el iPhone.

**Observaciones abiertas:**
1. vibración percibida con «Háptica del sistema» desactivada (S3-D);
2. destello percibido al golpear sin Perfect ni contacto, que cumple la condición de parada de §8 (observación de pausa);
3. bloqueos repetidos del iPhone, que obligaron a rehacer prechecks o intentos el 6 y el 9 de noviembre;
4. aperturas de la app fuera de la sesión, todas en P0.

## Recomendación de Claude

El registro respalda la cadena técnica de feedback de S3: el resultado final se entrega una sola vez, la presentación concuerda con él, las señales Valid y Failed aparecen en su caso y ningún intento sin contacto tiene presentación de contacto. La percepción del PO respalda el audio audible, la distinción entre golpe e intento al aire, la vibración en el golpe y el swing visible.

La sesión deja sin evaluar justo lo distintivo del Perfect: su nota, su destello y el contraste con un Valid. Tampoco quedan evaluados el silencio y el interruptor de háptica durante un impacto. Hay además dos percepciones que el registro no explica: la vibración con la háptica del sistema apagada y el destello sin Perfect.

**Claude recomienda no aceptar S3 solo con esta evidencia.** Propone una sesión complementaria breve con su propia predeclaración y autorización del PO, dirigida a cuatro puntos:
- obtener contacto Perfect y Valid;
- impactos aplicados con silencio y con `feedback.haptics=0`;
- pausa justo después de un contacto;
- aclarar las dos observaciones abiertas.

Las cuotas de esta sesión no se repiten. Otra opción es aceptar S3 con condiciones y dejar estos puntos registrados para S4. **Decide el PO.**

## Diagnóstico previo a la sesión complementaria

**9 de noviembre de 2026, Chile · Codex · solo lectura offline.** Encargo y dos frases literales del PO registrados primero en `7d00ef3`, sobre el head local/remoto comprobado `bacf39f7651e3fa1307c26b9a200b92498d22e86`, PR #40 abierta. No se accedió al iPhone ni se ejecutaron Unity, tests, builds o instalaciones. Se leyeron el informe completo, `po-log.txt` y los exports `after-D2`, `after-E` y `after-pause` de `Logs/evidence/s3-audio-revalidation-20261106/`. Las rutas de evidencia de esta sección son locales/ignoradas; no se publican sus archivos ni identificadores personales. Las líneas citadas son de los JSONL exportados y de las fuentes en esta rama. `Runtime`, `Core` y `Assets/Plugins/iOS` no tienen diferencias entre la fuente de la build instalada `097eab9` y `bacf39f`; no se atribuye al binario una ejecución nueva.

### Hechos cotejados en los registros

| Export / proceso (prefijo) | Líneas comprobadas | Hecho |
|---|---|---|
| `after-D2` / `metrics-20261110T001018492…` | 90, 130, 133–134, 142, 150 | Frontera DEV; M2-4; L16 Valid aplicado y presentado v2; L17/L18 Failed / Air sin contacto. Un aplicado y cero discrepancias en el proceso. |
| `after-E` / `metrics-20261110T001814969…` | 791, 831, 835, 906, 916, 924, 932 | Grupo «First Shot / feedback» desplegado, cambio de app haptics, cierre de preparación y cierre de cuota distintos; L51–53 Failed / Air, ningún aplicado ni discrepancia. |
| `after-pause` / `metrics-20261110T002854101…` | 81, 86, 88–90, 100, 104, 106, 149, 151–153, 157 | Frontera DEV; L18 Failed / outside-v y L21 Failed / before-k-in, ambos presentados Failed / Air v2 sin contacto. Rejected L20/L22; ningún aplicado, Perfect ni discrepancia en todo el proceso. |

Los SHA-256 de los tres procesos coinciden con los ya registrados: D `50eb5ae717adc891da035a236372cd30126e177e7c7cf5a18d30c63cf2533db4`, E `5b8a82634f221568b96d7f356c35216093f4f7654f2dfd88335d0bde6905ea00`, pausa `78620d40b83629913935847a113edebe17529c751cbb4a13c4da7c150cd24c93`. Esto coteja archivos históricos; no recalcula los hashes de configuración ni crea una medición física.

`po-log.txt` líneas 53–55 conserva «Vibración al golpear la pelota» y el ajuste «Sonidos y vibraciones → Háptica del sistema»; líneas 69–70 conserva «Se vio la raqueta, el sonido y el destello» y «Al golpear». Ambas percepciones siguen literales. Las líneas 100/104 del proceso de pausa sitúan pausa/reanudación a 00:30:39.350Z/00:30:45.647Z, después del Failed de L18 a 00:30:32.155Z. No prueban congelación justo después de contacto aplicado.

### a) Destello percibido sin Perfect

**Hechos del código y su alcance visual:**

| Elemento | Qué puede aparecer en un Failed / Air |
|---|---|
| Nota | «¡Al aire!», texto blanco sobre fondo oscuro cuando `timing.showNotes` está activo. La nota cambia con el resultado final; no tiene animación de destello propia. [ShotNoteView.cs:15–25](../../Assets/SquashClash/Runtime/Shot/ShotNoteView.cs#L15-L25), [ShotController.cs:176–187 y 227](../../Assets/SquashClash/Runtime/Shot/ShotController.cs#L176-L187). |
| Raqueta/brazo | El Swing aceptado inicia intención; el Failed final inicia `AirFollowThrough`, con recorrido de 0,28 s y retención de 0,3 s, seguido de recuperación. La rama Failed no entra en la pose `Impact` que busca el punto de contacto. Usa el material del jugador, sin encender un flash propio. [SwingView.cs:13, 48–57, 79–98 y 110–124](../../Assets/SquashClash/Runtime/Shot/SwingView.cs#L79-L98). |
| Mira de apuntado | M2-4 la muestra cuando tiene Aim y la dibuja como Frozen: rombo amarillo si el objetivo es válido, rojo si es inválido. No depende de Perfect/Valid/Failed; puede aparecer desde Began y se oculta al relanzar. Los dos Swing de la pausa tienen `hasAim=true`, `aimInvalid=false`, `lockOrigin=Began` (líneas 86/149), así que hay una ruta de aparición de mira amarilla anterior a la suelta, sin acreditar cada píxel renderizado. [GesturePreview.M24.cs:53–62](../../Assets/SquashClash/Runtime/Shot/GesturePreview.M24.cs#L53-L62), [AimReticleView.cs:37–57](../../Assets/SquashClash/Runtime/Shot/AimReticleView.cs#L37-L57), [GesturePreview.cs:136–141](../../Assets/SquashClash/Runtime/Shot/GesturePreview.cs#L136-L141). |
| Marcador temporal de contacto / anillo de timing | Se coloca en `Plan.PStar`, se contrae y cambia de cian a amarillo según el tiempo objetivo; es una ayuda previa, no prueba de contacto aplicado. Se oculta al consumir el Swing, también en Failed. [TimingRingView.cs:13–32](../../Assets/SquashClash/Runtime/Shot/TimingRingView.cs#L13-L32), [ShotController.cs:73–79 y 109–115](../../Assets/SquashClash/Runtime/Shot/ShotController.cs#L109-L115). |
| Cruz de objetivo factible | `ShowFeasible` dibuja una cruz blanca solo cuando la solución existe y bajó el objetivo. Failed sale antes de esa ruta; se limpia al relanzar. No es un marcador de golpe aplicado ni una explicación demostrada de esta pausa. [ShotController.cs:68 y 115–124](../../Assets/SquashClash/Runtime/Shot/ShotController.cs#L115-L124), [AimReticleView.cs:18–31](../../Assets/SquashClash/Runtime/Shot/AimReticleView.cs#L18-L31). |
| Marcas de impacto en pared | Pueden surgir con rebotes de la pelota aunque no haya golpe de raqueta: escuchan contactos con paredes no persistentes, aparecen como disco y se desvanecen en 1,5 s. No son Perfect. P0 conserva `aids.impactMarks=1` en el [manifiesto heredado, línea 90](../validation/2026-10-31-first-shot-s2-session27-hashes.md#L90). Esto demuestra una ruta posible, no que una marca concreta coincida con lo visto por el PO. [LegibilityAids.cs:47 y 101–127](../../Assets/SquashClash/Runtime/Ball/LegibilityAids.cs#L101-L127). |

**¿Puede PerfectFlashView activarse sin presentación Perfect?** En las rutas leídas, no: crea su disco oculto, se suscribe a `PresentationDelivered`, exige categoría Perfect, `HasContact=true` y lanzamiento actual, lo activa durante 0,12 s y lo oculta al relanzar. El gate solo construye Perfect para un registro con `Applied=true`; el controlador escribe `shot-presentation` antes de invocar a las vistas. No se encontró otro llamador que active ese disco. Fuentes: [PerfectFlashView.cs:20–38](../../Assets/SquashClash/Runtime/Shot/PerfectFlashView.cs#L20-L38), [ShotPresentation.cs:26–46](../../Assets/SquashClash/Core/Shot/ShotPresentation.cs#L26-L46), [ShotController.cs:182–187](../../Assets/SquashClash/Runtime/Shot/ShotController.cs#L182-L187). El proceso completo de pausa carece de Perfect: tampoco hay un Perfect anterior en ese proceso cuyo disco pudiera seguir visible. Esta deducción de código y log no sustituye una captura de pantalla del evento.

**¿Existe otro flash sin Perfect?** Sí en otra modalidad: `AimReticleView.FlashLock` blanquea y engrosa la mira 1,5 veces durante el intervalo pedido, sin consultar el resultado del golpe ([líneas 59–68](../../Assets/SquashClash/Runtime/Shot/AimReticleView.cs#L59-L68)). Su único llamador localizado está en `UpdateSlingView`, al bloquear Sling ([GesturePreview.Sling.cs:67–73](../../Assets/SquashClash/Runtime/Shot/GesturePreview.Sling.cs#L67-L73)). **No explica por sí solo este caso:** los logs y P0 usan `gesture.control=2`/M2-4; esa rama llama `OnM24Sample` y no `UpdateSlingView` ni `FlashLock` ([GesturePreview.cs:92 y 105–106](../../Assets/SquashClash/Runtime/Shot/GesturePreview.cs#L92-L106)). No se presenta ese flash de otra modalidad como defecto ni como causa observada.

**Hipótesis abiertas:** la aparición/cambio de la nota, la mira, la desaparición/cambio del anillo, una marca de pared o el movimiento breve de la raqueta pueden confundirse perceptivamente con un destello. No hay captura sincronizada ni localización/color/duración declarados que elijan entre ellas. «Al golpear» no identifica un elemento ni un instante técnico exacto. No se demuestra un flash Perfect indebido, ni se descarta la percepción. La parada histórica de §8 permanece. La sesión complementaria debe recoger primero las respuestas literales y después, con aclaraciones neutrales, dónde/cuándo se vio el destello; si vuelve a percibirse sin aplicación, detenerse conforme a §8.

### b) Vibración con «Háptica del sistema» desactivada

**Hechos del código.** [Haptics.cs:10–25](../../Assets/SquashClash/Runtime/Feedback/Haptics.cs#L10-L25) usa `DllImport("__Internal")` para `SquashHapticsPrepare` y `SquashHapticsImpact`; en iOS Player el backend es nativo y fuera de él es inoperante. `feedback.haptics`, capturado por lanzamiento, habilita o suprime ambas llamadas ([ShotController.cs:59–61](../../Assets/SquashClash/Runtime/Shot/ShotController.cs#L59-L61)). El puente [SquashHaptics.mm:4–18](../../Assets/Plugins/iOS/SquashHaptics.mm#L4-L18) reutiliza dos **`UIImpactFeedbackGenerator` de UIKit**, estilos Heavy/Light, en el hilo principal; `prepare` prepara y `impactOccurred` solicita el impacto. No usa `Handheld.Vibrate`, AudioServices ni un motor Core Haptics, y no consulta por su cuenta el interruptor «Háptica del sistema».

Failed solicita únicamente el swish y retorna antes de la háptica. Perfect solicita Firm/Heavy y Valid Discreet/Light cuando la app la permite ([ShotFeedback.cs:23–29](../../Assets/SquashClash/Runtime/Feedback/ShotFeedback.cs#L23-L29), [FeedbackMap.cs:7–12](../../Assets/SquashClash/Core/Shot/FeedbackMap.cs#L7-L12)). D/L16 acredita Valid aplicado; es coherente con la ruta de solicitud Light, con hash P0/háptica de app activa. El log no confirma ejecución del actuador ni identifica el origen físico de la sensación.

**Documentación oficial consultada el 9 de noviembre (Chile):**

- [Apple, UIImpactFeedbackGenerator](https://developer.apple.com/documentation/uikit/uiimpactfeedbackgenerator): describe la API para simular impactos y `impactOccurred()` como disparador; el ejemplo separa `prepare()` del impacto. [UIFeedbackGenerator](https://developer.apple.com/documentation/uikit/uifeedbackgenerator) describe las subclases. Se leyeron también las versiones Markdown oficiales enlazadas desde esas páginas. No especifican una correspondencia con el interruptor denominado «System Haptics».
- [Apple, guía de iPhone iOS 18, Change iPhone sounds and vibrations](https://support.apple.com/en-lamr/guide/iphone/iph07c867f28/18.0/ios/18.0): relaciona System Haptics apagado con vibración de llamadas entrantes y alertas. La guía es una referencia versionada, **no identificación del iOS instalado**. [Guía iOS 27 de la misma página](https://support.apple.com/guide/iphone/change-sounds-and-vibrations-iph07c867f28/27/ios/27) describe ese ajuste para controles e interacciones del sistema; tampoco especifica `UIImpactFeedbackGenerator` en una app. No se traslada una descripción de llamadas/controles a esta API ni una versión de la guía al teléfono probado.
- [Apple, WWDC24, What’s new in UIKit, 15:53–16:07](https://developer.apple.com/videos/play/wwdc2024/10118/): explica que el resultado del generador puede depender de plataforma y ajustes y producir háptica, audio, ambos o ninguno. Es una regla general, no la semántica documentada del interruptor concreto de S3-D.
- [Apple, Turn off vibration on iPhone](https://support.apple.com/guide/iphone/turn-off-vibration-iphd722c9100/ios): distingue desactivar todas las vibraciones mediante Accesibilidad → Tocar → Vibración. **No fue el ajuste declarado por el PO y no se propone cambiarlo para sustituir S3-D.**

**Conclusión limitada:** las fuentes consultadas no aclaran si apagar exactamente «Sonidos y vibraciones → Háptica del sistema» debe suprimir este `UIImpactFeedbackGenerator` en el iOS probado. No se infiere que iOS lo ignore ni que deba bloquearlo; tampoco se demuestra un defecto de la app. La sensación podría corresponder a la solicitud Light u otra fuente, sin causa identificada por los registros. **La observación sigue abierta y se incluye S3-I** en la adenda, manteniendo separado el interruptor propio `feedback.haptics=0` de S3-H.

### Estado y siguiente paso documental

No se ha demostrado un defecto nuevo ni se ha resuelto por inferencia ninguna de las dos percepciones. Los NotEvaluated y las 21 recepciones originales permanecen intactos. Se prepara una [adenda complementaria](../validation/2026-11-06-first-shot-s3-physical-predeclaration.md#s3-complementaria-20261109), para revisión de Claude coordinada por el PO en #40, sin fusionar. La sesión futura requiere autorización del PO al comenzar y operador designado; ningún bloque, precheck o restauración nuevos se declara ejecutado. Ante defecto demostrado u otra sorpresa se registra y se detiene, sin corregir en esta PR.

**Comprobación documental de la entrega.** Los cuatro Markdown autorizados conservan el contenido anterior de `bacf39f` byte a byte como prefijo, incluida la predeclaración original; las dos citas literales ya están en el primer commit. Verificadas las siete preguntas de §6, las dos de Perfect y la de pausa, sin cambios de texto, y 30 referencias locales nuevas (destinos y líneas de fuentes). Los 2.808 archivos de la carpeta de evidencia conservaron rutas y SHA-256 durante la preparación. Cotejo offline adicional de los tres archivos de presets/selección en `after-D2`, `after-E`, `after-pause` y `restored-final`: iguales al respaldo y referencia inicial, sin lectura nueva del teléfono. Delta propio limitado a AGENTS/aprobaciones/informe/predeclaración; sin ejecución de tests o check-release por tratarse exclusivamente de documentación.
