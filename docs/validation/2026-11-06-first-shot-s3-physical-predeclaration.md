# First Shot S3 — predeclaración de la validación física

**6 de noviembre de 2026, Chile.** Protocolo S3-A–E aprobado en la [decisión 8 del diseño](../reviews/2026-11-05-first-shot-s3-design-plan.md#decisiones-del-po) y [autorizado por el PO](../reviews/2026-10-25-first-shot-po-approvals.md#autorización-de-la-validación-física-de-s3--6-de-noviembre-de-2026-chile). Claude opera la sesión. Este documento se confirma en Git **antes del primer bloque** y no se modifica para hacer pasar resultados. No tuvo revisión independiente previa: se declara como desviación y se revisa junto con el informe.

Reglas heredadas sin cambio de la [predeclaración de S2](2026-10-31-first-shot-s2-session27-predeclaration.md): §2 (respaldo y perfil), §3.2 puntos 3–5 (precheck, frontera dev por cierre de DEV, relación de relojes, segundo plano), §7 (parada y conservación) y §8 (restauración). Aquí solo se fija lo propio de S3.

## 1. Fuente, build y perfil

- **Fuente:** `main` en `adb8c8f` (merge de PR #34). Producto idéntico al head aprobado `067ca91`. La build se compila desde un commit limpio de `firstshot/s3-validation` cuyo delta frente a `adb8c8f` es solo documentación; se comprueba con `git diff --stat adb8c8f` limitado a `docs/` y `AGENTS.md`.
- **Comprobaciones previas:** `scripts/check-release.sh` dev y prod con `gitDirty=false`. Las suites no se repiten: el producto es idéntico al de la regresión fresca 546/546 EditMode y 353/353 PlayMode sobre `067ca91`.
- **Build:** `scripts/build-ios.sh dev`, Release, IL2CPP, ARM64, no Development, con `SQUASH_DEV`. Etiqueta esperada `0.1.0+<commit> dev`.
- **Perfil:** cotejar el perfil incorporado al `.app` (equipo, bundle, dispositivo, creación y vencimiento) y la firma estricta antes de instalar. El perfil conocido vence el **2026-11-12T13:55:07Z (10:55 Chile)**. La sesión, incluida la restauración, termina antes. Si el perfil incorporado no es válido, no se instala y se consulta al PO.

## 2. Respaldo, instalación y arranque

1. Con la app PROD `0.1.0+8f030a8` aún instalada, exportar `Documents` del contenedor (metrics, spike y presets) a una ruta local ignorada.
2. Respaldar `P0-calib-1.json`, `Po-1.json` y `last-active.txt` con bytes y SHA-256. Referencia inicial:

| Archivo | Bytes | SHA-256 |
|---|---:|---|
| `P0-calib-1.json` | 3614 | `2f1295f201e238d9494bee285dd3f46e5bbb0f98b09ccde90fbd268f0874e4fd` |
| `Po-1.json` | 2843 | `9008c33ab89f82e669bc227a81b37ed641f69e4657ee70df6c89ff0c0edb1767` |
| `last-active.txt` | 10 | `5885ce8a7add389b2c82cb43f6944d7e2ef7ebeb0bffd77b5b635361a8a46087` |

   Si el respaldo no coincide con la referencia, no se instala y se consulta al PO.
3. Instalar la build dev sobre la PROD, sin desinstalar. Cotejar los tres archivos con el respaldo antes de abrir la app.
4. Arrancar un proceso nuevo y leer su log: build esperada, `presetName=P0-calib-1`, `presetFileHash=2f1295f2…`, **`effectiveHash=6ecfdfa78cbb`** y `pending=0`. Volver a cotejar los tres archivos después del arranque.

## 3. Hashes esperados, calculados fuera de Unity

Calculados reproduciendo `ParamSet.ComputeHash` sobre el [manifiesto canónico B de S2](2026-10-31-first-shot-s2-session27-hashes.md#manifiesto-canónico-b-completo), que reproduce `dd122831f592`, añadiendo la clave nueva de S3 `feedback.haptics`, en orden ordinal:

| Condición | Delta frente a B | H esperado | SHA-256 completo | Bytes / líneas |
|---|---|---|---|---|
| P0 con S3 (S3-A a S3-D) | `feedback.haptics=1` | **`6ecfdfa78cbb`** | `6ecfdfa78cbbc8dab17fe5deaab6e673cf6b9b5caaa1e68a7056eaa9cbe0e04c` | 2307 / 107 |
| S3-E, háptica de la app desactivada | `feedback.haptics=0` | **`4f33dff747fb`** | `4f33dff747fb8183355681c2938f125c099b5068e2865b598a2921ed319d6dbb` | 2307 / 107 |

La primera fila coincide con el valor declarado en la implementación de S3. P0 en S3 equivale a SL1, V100, cámara fija y M2-4. No se cambia ningún otro valor. Si el hash observado difiere del literal, se registra y se detiene, sin recalcular.

## 4. Procedimiento por bloque

1. **Proceso nuevo desde P0.** El operador relanza la app (`devicectl … --terminate-existing`). Para S3-E, el PO desactiva en DEV «App haptics» (`feedback.haptics`), sin guardar ni importar presets.
2. **Ajustes del teléfono.** Los cambia el PO, que declara literalmente su estado antes del bloque (timbre o silencio, háptica del sistema). El operador no accede a Ajustes.
3. **Precheck por exportación:** build, `presetName`, hash del último `shot-plan` igual al esperado del bloque, `pending=0`, sin pausa, `last-active` igual a P0 y los tres archivos iguales al respaldo. Se registra aceptado o rechazado.
4. **Frontera:** el PO abre DEV y lo cierra cuando está listo. El inicio es la línea `dev-control` con `control=dev-toggle`, `stage=result`, `accepted=true`, `reason=completed` y `devOpen=false`. La recepción 1 es el primer `LaunchId` cuyo `shot-plan` aparece después de esa línea; la cuota son los N `LaunchId` consecutivos. Lo anterior y lo posterior se cuenta como preparación o extras, fuera de cuota (`launchSerial = LaunchId − 1`).
5. **Fin:** el PO avisa «terminé S3-X». El operador exporta antes de cualquier relanzamiento.
6. **Preguntas:** antes de mostrar resultados o logs. Ver §6.

| Bloque | Cuota | Teléfono | Instrucción literal al PO |
|---|---:|---|---|
| **S3-A · Contacto** | 6 | Timbre activo, volumen audible, háptica del sistema activa | «Golpea a ritmo cómodo, como en S2.» |
| **S3-B · Sin contacto e intención** | 6 | Igual que S3-A | «Recepciones 1 y 2: apoya, carga un poco y cancela volviendo a la altura del apoyo. Recepción 3: resbalón hacia abajo. Recepción 4: resbalón principalmente lateral. Recepción 5: suelta claramente antes de tiempo. Recepción 6: suelta claramente tarde.» |
| **S3-C · Silencio** | 3 | Silencio activado | «Golpea a ritmo cómodo.» |
| **S3-D · Háptica del sistema desactivada** | 3 | Timbre activo; háptica del sistema desactivada si el iPhone ofrece el ajuste | «Golpea a ritmo cómodo.» |
| **S3-E · Interruptor de la app** | 3 | Timbre activo; háptica del sistema repuesta; «App haptics» desactivado en DEV | «Golpea a ritmo cómodo.» |

El orden de las tareas de S3-B concreta el diseño, que pedía dos cancelaciones, dos resbalones y dos sueltas fuera de ventana sin fijar el orden. Se registra el desenlace real aunque no coincida con la instrucción. Si en S3-D el ajuste no existe o no es reproducible, el bloque queda NotEvaluated, sin sustituirlo. Las cantidades delimitan la observación y no son criterios de éxito: no se repite una recepción para conseguir una categoría.

## 5. Observación opcional de pausa, fuera de cuota

El PO incluyó entre los objetivos la congelación y reanudación del feedback en pausa, que no está en S3-A–E. Se propone, **solo si el PO lo autoriza antes de empezar**, una observación breve después de S3-E y fuera de cuota: el PO pausa justo después de un golpe, observa raqueta y destello, y reanuda. Se pregunta literalmente: «¿Qué viste en la raqueta, el destello, el sonido y la vibración al pausar y al reanudar?». Si no se autoriza, el objetivo queda NotEvaluated en el iPhone, con la cobertura de Editor ya registrada.

## 6. Preguntas literales

Al cerrar **cada bloque**, antes de compartir logs, etiquetas técnicas o resultados, se envía un diálogo con un campo por pregunta, en este orden. La herramienta admite cuatro preguntas por diálogo, así que se envían dos diálogos seguidos (1–4 y 5–7). La herramienta exige al menos dos opciones: cada pregunta ofrece «No recuerdo» y «Nada que señalar» como salidas neutras, y la respuesta libre va en «Other». La respuesta se registra literal.

1. «Por número de recepción, ¿qué excepciones recuerdas: Apoyo sin querer, No fijó / falló u otra dificultad?»
2. «Por número de recepción, ¿qué sonido oíste, si oíste alguno?»
3. «Por número de recepción, ¿qué vibración sentiste, si sentiste alguna?»
4. «Por número de recepción, ¿qué nota, destello o movimiento de la raqueta viste, si viste alguno?»
5. «¿Hubo alguna recepción sin contacto en que percibiste sonido de impacto, vibración, destello o pose de contacto? Si recuerdas el número, indícalo.»
6. «Si hubo un intento al aire o tardío, ¿qué sonido y nota percibiste, si percibiste alguno?»
7. «Por número de recepción, ¿cuándo viste empezar el movimiento de la raqueta respecto de la suelta y qué viste después?»

Solo al cerrar S3-A se añade un tercer diálogo: «¿Un perfecto se siente claramente mejor?» y «¿Entendiste el resultado del golpe sin el panel?». Si falta un contraste observado (por ejemplo, ningún Perfect en el bloque), se conserva lo que diga el PO y el contraste técnico queda NotEvaluated.

## 7. Resultados objetivos que se informarán

Por bloque y recepción, desde el JSONL del proceso: `LaunchId`, gesto, resultado final y motivo (última versión de `shot-order`), contacto aplicado, líneas `shot-presentation` (categoría, `hasContact`, `fail`, versión) y `shot-presentation-discrepancy`. Comprobaciones descriptivas, sin umbrales nuevos:

- como máximo una presentación por identidad de intento;
- presentación Perfect o Valid solo con contacto aplicado;
- presentación Failed solo con resultado final Failed;
- ninguna presentación para cancelación, rechazo, NoOrder, SystemFault o NotEvaluable;
- cero discrepancias;
- en S3-E, `feedback.haptics=0` en el hash de cada recepción.

Lo que el log no acredita (que el sonido o la vibración se percibieran) se toma solo de las respuestas del PO. Las respuestas no se reinterpretan con métricas posteriores.

## 8. Parada y restauración

Además de §7 de S2, se detiene la sesión, se exporta y se restaura ante: señal de impacto, vibración, destello o pose de contacto sin aplicación; swish o nota de Fallido sin Failed final; señal duplicada; contradicción de versiones; o hash distinto del esperado.

Al terminar o interrumpir: exportar, pedir al PO que reponga los ajustes del teléfono, relanzar en proceso nuevo con P0 (la desactivación temporal de S3-E no se guarda), verificar `6ecfdfa78cbb` y `pending=0`, y cotejar los tres archivos byte a byte y por SHA-256 contra el respaldo y la referencia inicial, también después del arranque. La build dev de S3 queda instalada en lugar de la PROD de S2, sin reinstalar por inferencia.

El informe se entrega en una PR para revisión independiente, sin fusionar, con resultados por bloque, respuestas literales, problemas, verificaciones y la recomendación de Claude.

## Adenda del 6 de noviembre de 2026 · Prueba breve de sonido y repetición con audio corregido

Codex opera la continuación desde `firstshot/s3-validation` tras integrar `main` en `27058e8` (PR #35). El delta frente a ese main es documental. La nueva DEV se instala sobre la DEV anterior `0.1.0+fab60ef dev`, con el mismo procedimiento de exportación, respaldo, cotejo previo a la apertura y verificación posterior al arranque de §2. La corrección de audio conserva los hashes de §3. La regresión revisada de PR #35 fue 546/546 EditMode y 354/354 PlayMode. Esta adenda se confirma en Git antes del nuevo build y del acceso al dispositivo.

1. Tras verificar la instalación, preparar un proceso nuevo en P0-calib-1, hash `6ecfdfa78cbb`, cero pendientes, sin pausa y con los tres archivos cotejados. El PO declara timbre activo y volumen multimedia audible comprobado dentro de la app. Juega con el iPhone en la mano.
2. Para separar los intentos de la preparación, el PO abre y cierra DEV. La frontera y la primera recepción se identifican como en §4. El PO golpea a ritmo cómodo hasta percibir el primer contacto o llegar a **seis recepciones**, lo que ocurra primero, y avisa al terminar. Se exporta antes de relanzar. Se coteja al menos un contacto aplicado en la ventana. Los intentos y extras se conservan fuera de cuota de S3-A–E.
3. Antes de mostrar resultados se pregunta literalmente «¿Oíste un sonido de impacto al golpear?». Si no se obtiene contacto aplicado en la ventana, se registra la prueba como NotEvaluated. Si hay contacto aplicado y el PO no oye impacto, se exporta, restaura y detiene según §8. Solo con contacto aplicado, confirmación de impacto audible y autorización explícita del PO para iniciar S3-A se abre la nueva cuota.
4. Repetir S3-A–E desde cero conforme a §4–§8. Recordar al PO antes de cada bloque que debe abrir y cerrar DEV para iniciar. El S3-A parcial previo queda separado. La ausencia de audio cuando debe oírse activa la parada. La condición de silencio de S3-C se conserva.
5. El PO autorizó la observación de pausa de §5 («Sí, incluirla»). Se realiza después de S3-E y fuera de cuota. La sesión completa, incluida la restauración, debe terminar dentro de la vigencia del perfil incorporado.
