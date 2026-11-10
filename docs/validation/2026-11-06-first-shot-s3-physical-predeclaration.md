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

<a id="s3-complementaria-20261109"></a>

## Adenda del 9 de noviembre de 2026 · Sesión complementaria S3-F–I y S3-P

**Predeclaración documental, no ejecución ni aceptación de S3.** Se añade sin modificar el texto anterior. Las dos decisiones literales del PO constan en el primer commit `7d00ef3`, en AGENTS.md y en el [registro de aprobaciones](../reviews/2026-10-25-first-shot-po-approvals.md). El [diagnóstico offline](../reviews/2026-11-06-first-shot-s3-physical-validation.md#diagnóstico-previo-a-la-sesión-complementaria) no demuestra defecto nuevo, pero conserva abiertas las dos percepciones. La parada histórica no se convierte en Pass. Esta adenda debe estar **confirmada en Git y entregada en PR #40 antes de cualquier acceso al iPhone**. El PO coordina la revisión de Claude y decide quién opera; la sesión requiere su autorización expresa **al momento de empezar**. «Mañana» en el encargo significa **10 de noviembre de 2026, Chile**, sin hora acordada. Hoy no se accede al teléfono ni se hace build o instalación.

### C1. Objetivo, fuente y reserva de tiempo

Cubrir contraste Perfect/Valid, impacto en silencio, impacto con háptica de app desactivada, pausa justo después de contacto aplicado y las dos observaciones abiertas. **No repetir ni sustituir A–E**: son bloques nuevos con corte anticipado predeclarado. No cambian spec, planes, escenarios, bandas, criterios, umbrales, defaults o presets.

Se usa la app ya instalada **`0.1.0+097eab9 dev`**, producto de `main 27058e8`; no se prepara otra build ni instalación. Última restauración histórica observada: `2026-11-10T00:33:41.2169830Z`, P0 `6ecfdfa78cbb`, cero pendientes y tres archivos cotejados; debe verificarse de nuevo al comenzar, sin presumir que el teléfono sigue igual. Hashes **literales existentes**, sin recalcular: P0 `6ecfdfa78cbb`; solo S3-H `4f33dff747fb`.

Reserva de **unos 20 minutos**: aproximadamente 3 para autorización/preparación, 13 para bloques/prechecks/preguntas y **4 reservados para exportación y restauración**. Es una previsión operativa, no criterio de éxito ni garantía de completar todos los bloques. Al minuto 16 se cierra la adquisición y se pasa a restaurar; si preguntas/exportaciones tardan más, los bloques no alcanzados quedan NotEvaluated. No comprimir preguntas, omitir prechecks o ampliar cuotas para terminar. Si la restauración tarda más, se continúa hasta verificarla, dentro de la vigencia del perfil; no declarar cierre con restauración pendiente.

Perfil incorporado conocido: **vence el 12 de noviembre de 2026 a las `13:55:07Z` = `10:55:07 America/Santiago`**. La sesión propuesta es el día 10; **incluida toda restauración, debe terminar antes de ese vencimiento absoluto**, también si se reprograma. El operador revalida la evidencia local de la firma/perfil incorporado y el margen disponible al empezar. Si fuente, build, perfil o ventana completa no coinciden/no caben, registrar y detenerse; no renovar, compilar ni instalar por inferencia.

### C2. Preparación y precheck obligatorio antes de cada bloque

1. **Solo con PO presente y autorización recibida.** Registrar fecha/hora Chile/UTC, operador, autorización literal y commit de esta adenda. Exportar antes de nuevas aperturas, por la retención de logs. Preservar fuera del contenedor las exportaciones nuevas con etiquetas únicas; no sobrescribir A–E, sus exports ni `po-log.txt`. Comparar `P0-calib-1.json`, `Po-1.json` y `last-active.txt` byte a byte y por SHA-256 contra el respaldo `backup-before-install` y la referencia inicial de §2. Si difieren, conservar y detenerse; no recrear presets ni seleccionar otro para encubrir la diferencia.
2. Proponer al PO: **«Para evitar interrupciones, puedes dejar temporalmente Ajustes → Pantalla y brillo → Bloqueo automático en Nunca y reponer tu ajuste al terminar»**. Él cambia Ajustes; el operador no entra. Registrar estado anterior y decisión literal, sin asumir que lo hizo ni que la opción existe. Si no puede/quiere, mantener el teléfono despierto; un bloqueo no invalida las reglas de interrupción. iPhone en mano, Duplicación cerrada, app en primer plano y pulgar izquierdo dominante, como la sesión anterior; confirmar condición, sin atribuir al Mac sonido o vibración del teléfono.
3. **Proceso nuevo desde P0 para cada bloque**, incluida S3-P. Export previo antes de relanzar; cotejar lanzamiento nuevo, `0.1.0+097eab9 dev`, nombre y hash de archivo P0 de §2, `effectiveHash=6ecfdfa78cbb`, `pending=0` y los tres archivos iguales después del arranque. Conservar registro de preparación separado de la cuota. No guardar/importar presets ni cambiar receta, ventana, cámara, tempo, mano o ayudas. Para H se verifica primero ese arranque P0 y luego se cambia solo la háptica en memoria dentro del mismo proceso (C3).
4. El PO declara literalmente **timbre/silencio, volumen/ruta y háptica del sistema** según C3 después de cualquier visita a Ajustes. Si el cambio deja la app en segundo plano, el proceso nuevo y el precheck completo se hacen **después de volver**. Comprobar Jugando, sin pausa manual ni «Toca para continuar», foco, P2, SL1, M2-4, V100/T3 no suave, cámara fija y mira/anillo visibles, como §3.2 heredado. No iniciar si el PO no identifica los elementos. Mantener `feedback.haptics=1` salvo H.
5. Exportar **el proceso que realmente se medirá**. Cotejar último `shot-plan`: hash efectivo y hash de recepción iguales al literal de C3, cero pendientes, foco/sin pausa y tres archivos sin cambios. Registrar precheck aceptado/rechazado y su motivo, proceso, build, preset, hash esperado/observado y plan. El helper `precheck.py` comprueba arranque/último plan, pero **no sustituye** la comprobación de foco/pausa/overlay/condición/archivos. Ningún gesto solicitado antes de aceptarlo.
6. **Frontera por cierre DEV del PO**, exactamente §4: con precheck aceptado, el PO abre DEV si está cerrado y lo cierra una sola vez cuando el operador da la señal. Guardar `M_s`/`M_r`, línea `dev-control` completada/aceptada, `devOpen=false`, foco, sin pausa, hash esperado y `timeReal`/`ts`. La recepción 1 es el primer `LaunchId` cuyo `shot-plan` empieza después de ese cierre. El lanzamiento en curso al cerrar es preparación. No usar como frontera el cierre con que se preparó H ni una hora del chat. Una frontera ambigua no se elige a conveniencia: conservar y detener el bloque.
7. Conteo **por recepciones consecutivas**, no por aciertos ni número de gestos. NoOrder, omisiones, cancelaciones y rechazos consumen su recepción. Cada intento conserva identidad/versión; `launchSerial=LaunchId−1`. Todo lo anterior/posterior al corte son preparación/extras con sus resultados preservados. Al terminar, el PO deja de gesticular y avisa «Terminé S3-X»; exportar antes de cualquier relanzamiento, preguntar conforme a C4 y auditar antes del siguiente bloque.

Bloqueo, segundo plano, cambio de proceso o contexto interrumpen: retirar señal, exportar y conservar parcial, restaurar P0 en proceso nuevo y repetir precheck completo antes de cualquier otro bloque autorizado. **No reanudar ni repetir automáticamente un parcial**: requiere decisión literal del PO y registro adicional, fuera de los máximos de esta adenda. Ante defecto o sorpresa, rige C6.

### C3. Orden, máximos y cortes predeclarados

| Orden / bloque | Condición del teléfono y hash | Corte de cuota | Instrucción literal al PO |
|---|---|---|---|
| 1 · **S3-F · Contraste Perfect/Valid** | Timbre activo, volumen audible, háptica del sistema activa; app haptics=1; `6ecfdfa78cbb` | Al completar el primer par de categorías Perfect y Valid **ambas aplicadas**, o **12 recepciones**, lo que ocurra primero. | «Golpea a ritmo cómodo buscando el timing. Son como máximo doce recepciones; omisiones y cancelaciones cuentan. Te avisaré si el bloque termina antes. Al terminar, deja de jugar.» |
| 2 · **S3-P · Pausa fuera de F–I** | Mismos ajustes que F; proceso propio/P0 `6ecfdfa78cbb` | Primer contacto aplicado y observación pausa/reanudación; si no aparece, **máximo 4 recepciones**. | «Golpea a ritmo cómodo. Justo después del primer contacto que percibas, pausa, observa la raqueta y lo que siga visible, y reanuda. Luego deja de jugar. Son como máximo cuatro recepciones; si pausaste tras un intento sin contacto, termina igualmente y avísame.» |
| 3 · **S3-G · Silencio con impacto** | Silencio activado; háptica del sistema activa, app haptics=1; `6ecfdfa78cbb` | **Primer contacto aplicado o 4 recepciones**, lo primero. | «Con modo silencio activado, golpea a ritmo cómodo. Son como máximo cuatro recepciones; te avisaré si termina antes. Al terminar, deja de jugar.» |
| 4 · **S3-H · Háptica de app desactivada** | Timbre activo/volumen audible; háptica del sistema activa; **app haptics=0**, `4f33dff747fb` | **Primer contacto aplicado o 4 recepciones**, lo primero. | «Con App haptics apagado, golpea a ritmo cómodo. Son como máximo cuatro recepciones; te avisaré si termina antes. Al terminar, deja de jugar.» |
| 5 · **S3-I · Háptica del sistema desactivada** | Timbre activo/volumen audible; sistema desactivado exactamente en «Sonidos y vibraciones → Háptica del sistema»; app haptics=1; `6ecfdfa78cbb` | **Primer contacto aplicado o 4 recepciones**, lo primero. | «Con Háptica del sistema desactivada, golpea a ritmo cómodo. Son como máximo cuatro recepciones; te avisaré si termina antes. Al terminar, deja de jugar.» |

**Justificación.** Se da a F un máximo mayor por necesitar dos categorías, sin garantía de obtener Perfect en SL1. Los cuatro bloques restantes tienen máximo 4 cada uno para limitar la sesión: **28 recepciones en total como techo**, sin contar preparación/extras, que se conservan. S3-P va después de F para usar la condición audible/háptica de referencia y separar su pausa del denominador F. G→H→I mantiene los controles de silencio, app y sistema en bloques propios; H devuelve timbre y I exige un proceso nuevo que restaura app haptics=1. Es un orden fijo, no aleatorizado: se declara posible sesgo de aprendizaje/orden. Las cantidades y cortes definen observación, no nuevos criterios de aceptación. S3-I se incluye **porque el diagnóstico no resuelve** la relación del interruptor del sistema con UIKit; no presupone que la vibración deba desaparecer.

**Preparación específica de H, sin repetir el fallo de S3-E.** Después de comprobar el proceso nuevo en P0: «Abre DEV → pestaña de parámetros → despliega el grupo **First Shot / feedback** → apaga **App haptics** (`feedback.haptics`). No guardes ni importes presets. Cierra DEV y no golpees todavía». Esperar un lanzamiento con `4f33dff747fb` y cero pendientes. **No relanzar el proceso después del cambio**, pues se perdería el valor temporal. Exportar y hacer el precheck definitivo en ese mismo proceso. Tras aceptarlo, pedir otra apertura/cierre DEV para la frontera medida, separada del cierre de preparación.

**Cómo cortar sin condicionar las respuestas.** El operador correlaciona privadamente por proceso/LaunchId el `shot` final `Applied=true` y su `shot-presentation` concordante; una orden provisional o contacto percibido no demuestra aplicación. Usa exports de monitoreo separados. Da solo una señal neutra de fin, sin revelar categoría, recuentos de aciertos o resultados antes de C4. El corte objetivo es la primera recepción que cumple la regla, aunque la señal llegue después: conservar las recepciones posteriores como extras, sin escoger otras ni ampliar hasta conseguir un resultado. Si no puede determinarse el corte con los registros, declarar incompleto/NotEvaluated, sin fabricarlo desde el recuerdo. No decir al PO qué respuesta se espera en silencio, app haptics o sistema.

**Pausa.** S3-P es fuera de las cuotas F–I, con precheck/frontera propios. El PO ejecuta la pausa por percepción inmediata para no esperar la latencia de exportación; después se comprueba la aplicación y la secuencia temporal del mismo lanzamiento. Un intento sin contacto o una pausa posterior, como la histórica de L20, **no acreditan** este objetivo y no se sustituyen. No se añade tolerancia temporal numérica. Se registra literalmente lo observado y los tiempos. Si el primer contacto es Valid, puede evaluarse la raqueta; la congelación del destello Perfect queda NotEvaluated. Incluso con Perfect, si el disco de 0,12 s ya no estaba visible al pausar, no se declara congelación del flash. No repetir para conseguirlo. El bloque termina al observar la pausa/reanudación asociada al primer contacto aplicado, al llegar al máximo o al primer intento de pausa no asociado a contacto, conservado como parcial; la regla de parada prevalece. Si se registra el primer contacto aplicado y el PO no pausó tras él, cerrar igualmente: pausa NotEvaluated, sin buscar un segundo contacto para sustituirlo.

### C4. Preguntas por diálogo: literales, una a una y antes de resultados

Esta adenda concreta el encargo actual: **una pregunta por diálogo y esperar la respuesta antes de la siguiente**, en lugar de los diálogos agrupados históricos de §6. Sin opciones preseleccionadas ni respuestas fabricadas; conservar «No recuerdo», «Nada que señalar» o ausencia de respuesta tal como ocurran. Después de **cada bloque**, incluido P, enviar las siete de §6 literalmente y en este orden:

1. «Por número de recepción, ¿qué excepciones recuerdas: Apoyo sin querer, No fijó / falló u otra dificultad?»
2. «Por número de recepción, ¿qué sonido oíste, si oíste alguno?»
3. «Por número de recepción, ¿qué vibración sentiste, si sentiste alguna?»
4. «Por número de recepción, ¿qué nota, destello o movimiento de la raqueta viste, si viste alguno?»
5. «¿Hubo alguna recepción sin contacto en que percibiste sonido de impacto, vibración, destello o pose de contacto? Si recuerdas el número, indícalo.»
6. «Si hubo un intento al aire o tardío, ¿qué sonido y nota percibiste, si percibiste alguno?»
7. «Por número de recepción, ¿cuándo viste empezar el movimiento de la raqueta respecto de la suelta y qué viste después?»

Tras F, añadir, cada una en su diálogo: **«¿Un perfecto se siente claramente mejor?»** y **«¿Entendiste el resultado del golpe sin el panel?»**. Se preguntan sin revelar si se obtuvo Perfect; si falta el contraste técnico, queda NotEvaluated aunque haya una respuesta global positiva.

Tras las siete de P, preguntar literalmente §5: **«¿Qué viste en la raqueta, el destello, el sonido y la vibración al pausar y al reanudar?»**. Si no pudo hacer la pausa, conservar esa respuesta y el objetivo no evaluado.

Solo después de las preguntas base se permiten aclaraciones **neutrales**, una a una, por ejemplo: «¿Dónde viste el destello?» y «¿En qué momento apareció?». Para I: «¿Qué ajuste cambiaste y en qué ruta?». No ofrecer como opciones nota/mira/raqueta/Perfect ni sugerir que un elemento concreto fue la causa. No mostrar las hipótesis del diagnóstico como explicación previa al PO. Las respuestas van literales, con bloque, número si lo recuerda, hora y pregunta; sin correlación individual inventada. Si una respuesta activa §8, suspender los bloques y conservar las preguntas pendientes como tales; ninguna regla obliga a seguir jugando para aclarar la percepción.

### C5. Herramientas existentes, auditoría y evidencia separada

Los helpers locales del 6 de noviembre en `Logs/evidence/s3-audio-revalidation-20261106/helpers/` se leen y reutilizan **solo al autorizarse la sesión**; no se ejecutaron contra el dispositivo en esta preparación. `export.sh`/`launch.sh` acceden al iPhone; `precheck.py`, `cotejo.py` y `audit.py` leen exportaciones. No modificar ni versionar estos helpers ni `scripts/device.env`, ni copiar sus valores al informe. No ejecutar AutoQA como sustituto de la percepción del PO.

- Usar etiquetas nuevas e inequívocas, por ejemplo `complement-20261110-before-F`, `complement-20261110-pre-F`, `complement-20261110-after-F`, con sufijo de intento si se necesita. Conservar respuestas nuevas en un registro local separado, sin editar `po-log.txt` original. El mismo directorio raíz de los helpers permite trabajar sin cambiarlos; todos estos archivos siguen ignorados.
- `precheck.py <export-actual> <export-anterior> <hash-literal>` exige que el proceso actual **no exista en el export anterior**. Para cada proceso nuevo conservar ese export previo. **En H, el anterior será el export de G tomado antes de crear el proceso H**, no el precheck P0 de H ni uno posterior al cambio. El export actual de H contiene arranque P0 y último plan con `4f33dff747fb`; el chequeo definitivo se hace allí. Es la misma regla documentada en S3-E al usar `after-D2`. Si el proceso ya aparecía en el anterior, seleccionar la referencia cronológica correcta conservada antes de crearlo; no editar los logs ni suprimir la aserción.
- El export de fin debe conservar como prefijo el JSONL del precheck y la frontera medida debe ser única después de ese prefijo. Auditar con `audit.py` usando **N realmente incluido hasta el primer corte predeclarado**, no siempre el máximo. Verificar por lectura del log completo que ninguna recepción anterior satisfacía el corte. El helper imprime recepciones y errores; **no calcula el corte adaptativo ni convierte un parcial en cuota completa**. En P comprobar además `pause` aceptado/pausado y reanudación, y contacto/Perfect y su temporalidad por separado.
- Informar por recepción gesto, última versión final, aplicado, presentación/categoría/hasContact/fail/versión, discrepancias, hash de lanzamiento/gesto y pendientes. Como §7: una presentación como máximo por intento, Perfect/Valid solo con aplicación, Failed solo con Failed final, ninguna para cancelación/rechazo/NoOrder/fallo/no evaluable, cero discrepancias. Preparación, extras, intentos interrumpidos y preguntas sin respuesta quedan visibles. Ningún NotEvaluated cuenta como Pass.

### C6. Parada, restauración y entrega

**§8 y las reglas heredadas de parada siguen íntegras.** Ante señal de impacto, vibración, destello o pose de contacto sin aplicación; swish/nota de Failed sin Failed final; duplicado; contradicción de versiones; hash distinto; defecto demostrado u otra sorpresa: detener adquisición, exportar, registrar sin atribuir causa y restaurar. No corregir producto ni retocar parámetros/criterios en #40. La duda histórica del destello no autoriza a ignorar una nueva percepción equivalente. Una vibración con contacto en I se registra como observación de ese ajuste; no se convierte por sí sola en prueba de defecto de UIKit.

Al terminar o interrumpir, exportar **antes** de relanzar; el PO repone timbre/háptica del sistema y el bloqueo automático a sus estados previos, y declara qué repuso. Después de volver de Ajustes, **restaurar P0 en un proceso nuevo**, sin guardar la configuración temporal H: build `0.1.0+097eab9 dev`, nombre/hash de archivo P0 de §2, **hash efectivo y último plan `6ecfdfa78cbb`, cero pendientes**. Cotejar `P0-calib-1.json`, `Po-1.json`, `last-active.txt` **byte a byte y por SHA-256** con respaldo y referencia inicial antes y después del arranque. Si no se puede verificar, declarar restauración pendiente y detener el cierre para el PO, sin afirmar éxito. No dejar H activo por falta de tiempo; no cambiar archivos para hacerlos coincidir.

Actualizar informe/aprobaciones/AGENTS con autorización, operador, condiciones, fronteras, cortes, todos los resultados/juicios/limitaciones y restauración. Commit/push solo documental en #40, revisión de Claude coordinada por el PO y **sin fusionar**. S3 no queda aceptado por completar bloques: decide el PO. Sin #32, S4, First Rally ni First Match.

### C7. Guion de arranque para el operador futuro

> Continúa en `firstshot/s3-validation`, worktree `~/.codex/worktrees/s3-validation/Squash Clash`, PR #40 abierta y sin fusionar. Lee esta adenda C1–C6, el diagnóstico y el registro de aprobaciones; comprueba head/árbol y que la adenda ya está confirmada en Git. No accedas al iPhone hasta recibir autorización del PO al comenzar y su designación de operador. Usa solo la app instalada `0.1.0+097eab9 dev`, sin build/instalación. Propón al PO bloqueo automático Nunca temporal y registra su decisión; él opera Ajustes. Revalida el perfil conocido hasta 12nov13:55:07Z/10:55:07Chile y reserva restauración. Exporta/respalda/coteja antes de relanzar. Ejecuta solo F→P→G→H→I con precheck completo por proceso y frontera por cierre DEV del PO, cortes predeclarados y máximo global de 28 recepciones. En H despliega First Shot / feedback; coteja `4f33dff747fb` en el mismo proceso, con export anterior donde ese proceso aún no existía. Resto `6ecfdfa78cbb`; no recalcules hashes. Preguntas C4 literales, una a una y antes de mostrar resultados. Conserva todo, incluidos extras y parciales; no repitas para conseguir categoría. Detén ante §8/defecto/sorpresa y restaura P0 en proceso nuevo con cero pendientes y tres archivos idénticos al respaldo y referencia inicial. Entrega documental en #40 para Claude/PO, sin fusionar ni aceptar S3 por inferencia.
